# Query

To implement a new query helper create an IQueryable<TModel> extension which modifies the query.

Do not call `.ToList()` cause it'll trigger a round trip to the database and you don't want that to happen just yet.
