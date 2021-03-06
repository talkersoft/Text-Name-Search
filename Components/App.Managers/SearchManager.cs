﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Contracts;
using App.Core.Contracts;
using App.Entities.Search;
using App.Managers.Base;
using App.Store.Contracts;

namespace App.Managers
{
    public class SearchManager : Manager, ISearchManager
    {
        public SearchManager(
            IDataStore store,
            IConfigurationStore config)
           : base(store)
        {
            this.Timeout = config.GetConfiguration<int>("SQLTimeout");
        }

        public async Task<IEnumerable<Search>> GetAll()
        {
            var query = await this.GetQueryAsync();
            return await this.Store.GetListAsync<Search>(query);
        }

        public async Task<IEnumerable<SearchResultsView>> GetView()
        {
            var query = await this.GetQueryAsync();
            return await this.Store.GetListAsync<SearchResultsView>(query);
        }

        public async Task<Search> Get(Guid identifier)
        {
            var query = await this.GetQueryAsync();
            return await this.Store.FirstOrDefaultAsync<Search>(
                query,
                new
                {
                   SearchIdentifier = identifier,
                });
        }

        public async Task<Guid> Add(Search search)
        {
            var query = await this.GetQueryAsync();
            return await this.Store.ExecuteScalerAsync<Guid>(
                query,
                new
                {
                    search.FirstName,
                    search.LastName,
                    search.MiddleName,
                    search.CreatedBy,
                });
        }

        public async Task<IEnumerable<Result>> Search(List<string> nameSequences, string[] contentArray)
        {
            List<Result> results = new List<Result>();
            foreach (var content in contentArray)
            {
                var items = await this.Search(nameSequences, content, 0, 0, 0, null);
                if (items != null)
                {
                    results.AddRange(items);
                }
            }

            return results;
        }

        private async Task<IEnumerable<Result>> Search(List<string> nameSequences, string content, int arrayIndex, int occurences, int lastSearchIndex, List<Result> results)
        {
            int searchIndex = 0;
            if (results == null)
            {
                results = new List<Result>();
            }

            if (lastSearchIndex == 0)
            {
                searchIndex = content.IndexOf(nameSequences[arrayIndex]);
            }
            else
            {
                searchIndex = content.IndexOf(nameSequences[arrayIndex], lastSearchIndex + nameSequences[arrayIndex].Length);
            }

            if (searchIndex > 0)
            {
                return await this.Search(nameSequences, content, arrayIndex, occurences + 1, searchIndex, results);
            }
            else
            {
                // No More matches
                if (occurences > 0)
                {
                    results.Add(new Result()
                    {
                        Occurrences = occurences,
                        SearchText = nameSequences[arrayIndex],
                        Created = DateTime.UtcNow,
                        CreatedBy = 1,
                    });
                }

                if (arrayIndex + 1 < nameSequences.Count)
                {
                    return await this.Search(nameSequences, content, arrayIndex + 1, 0, 0, results);
                }
                else
                {
                    return results;
                }
            }
        }
    }
}
