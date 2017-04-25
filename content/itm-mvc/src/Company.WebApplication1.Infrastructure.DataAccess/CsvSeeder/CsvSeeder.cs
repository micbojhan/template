using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace itmyosample.Infrastructure.DataAccess.CsvSeeder
{
    public static class CsvSeeder
    {
        private static CsvConfiguration _configuration = new CsvConfiguration
        {
            CultureInfo = new CultureInfo("da-DK") // We force danish locale, to support consistant seed file datetime format.
        };
        
        /// <summary>
        /// Seeds a DBSet from a CSV file that will be read from the specified stream
        /// </summary>
        /// <typeparam name="T">The type of entity to load</typeparam>
        /// <param name="dbSet">The DbSet to populate</param>
        /// <param name="stream">The stream containing the CSV file contents</param>
        /// <param name="identifierExpression">An expression specifying the properties that should be used when determining whether an Add or Update operation should be performed.</param>
        /// <param name="additionalMapping">Any additonal complex mappings required</param>
        public static void SeedFromStream<T>(this DbSet<T> dbSet, Stream stream, params CsvColumnMapping<T>[] additionalMapping) where T : class
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                CsvReader csvReader = new CsvReader(reader, _configuration);
                var map = csvReader.Configuration.AutoMap<T>();
                map.ReferenceMaps.Clear();
                csvReader.Configuration.RegisterClassMap(map);
                csvReader.Configuration.WillThrowOnMissingField = false;
                while (csvReader.Read())
                {
                    var entity = csvReader.GetRecord<T>();
                    foreach (CsvColumnMapping<T> csvColumnMapping in additionalMapping)
                    {
                        csvColumnMapping.Execute(entity, csvReader.GetField(csvColumnMapping.CsvColumnName));
                    }
                    dbSet.Add(entity);
                }
            }
        }

        /// <summary>
        /// Seeds a DBSet from a CSV file that will be read from the specified file name
        /// </summary>
        /// <typeparam name="T">The type of entity to load</typeparam>
        /// <param name="dbSet">The DbSet to populate</param>
        /// <param name="fileName">The name of the file containing the CSV file contents</param>
        /// <param name="identifierExpression">An expression specifying the properties that should be used when determining whether an Add or Update operation should be performed.</param>
        /// <param name="additionalMapping">Any additonal complex mappings required</param>
        public static void SeedFromFile<T>(this DbSet<T> dbSet, string fileName, params CsvColumnMapping<T>[] additionalMapping) where T : class
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                SeedFromStream(dbSet, stream, additionalMapping);
            }
        }
    }
}
