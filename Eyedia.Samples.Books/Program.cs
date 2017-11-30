using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eyedia.Aarbac.Framework;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using GenericParsing;
using Eyedia.Samples.Books;

namespace ConTest
{
    class Program
    {
        static void Main(string[] args)
        {                   
            var parser = new GenericParserAdapter(
                Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, 
                "Data", "500.csv"));
            parser.FirstRowHasHeader = true;
            DataTable t = parser.GetDataTable();
            foreach (DataRow r in t.Rows)
            {
                using (var ctx = new BooksEntities())
                {
                    string sm = r["State"].ToString();
                    List<State> states = ctx.States.Where(s => s.ShortName.Equals(sm)).ToList();
                    State state = null;
                    if (states.Count > 0)
                        state = states[0];

                    if (state == null)                   
                        continue;

                    string cm = r["City"].ToString();
                    List<City> cities = ctx.Cities.Where(c => c.Name.Equals(cm)).ToList();                    
                    City city = null;
                    if (cities.Count > 0)
                        city = cities[0];

                    if (city == null)
                    {
                        City newCity = new City();
                        newCity.Name =cm;
                        newCity.StateId = state.StateId;
                        city = ctx.Cities.Add(newCity);
                    }

                    string zcc = r["ZIP"].ToString();
                    List<ZipCode> zcs = ctx.ZipCodes.Where(z => z.ZipCode1.Equals(zcc)).ToList();
                    ZipCode zc = null;
                    if (zcs.Count > 0)
                        zc = zcs[0];

                    if (zc == null)
                    {
                        ZipCode newzc = new ZipCode();
                        newzc.ZipCode1 =zcc;
                        newzc.CityId = city.CityId;
                        zc = ctx.ZipCodes.Add(newzc);
                        
                    }

                    string name = r[0].ToString() + " " + r[1].ToString();
                    List<Author> authors = ctx.Authors.Where(a => a.Name.Equals(name)).ToList();
                    Author author = null;
                    if (authors.Count > 0)
                        author = authors[0];

                    if (author == null)
                    {
                        Author newAuthor = new Author();
                        newAuthor.Name = name;
                        newAuthor.SSN = UTF8Encoding.UTF8.GetBytes(RandomString(11));
                        newAuthor.ZipCodeId = zc.ZipCodeId;
                        author = ctx.Authors.Add(newAuthor);
                    }
                    ctx.SaveChanges();
                }
            }
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region GetCityId
        private static void GetCityStateZipIds(string connStr)
        {
            var parser = new GenericParserAdapter("c:\\temp\\500.csv");
            parser.FirstRowHasHeader = true;
            DataTable t = parser.GetDataTable();

            DataTable newT = new DataTable();

            newT.Columns.Add("Name");
            newT.Columns.Add("CityId");
            newT.Columns.Add("StateId");
            newT.Columns.Add("ZipCodeId");

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();
                foreach (DataRow r in t.Rows)
                {
                    DataRow newr = newT.NewRow();
                    newr[0] = r[0].ToString() + " " + r[1].ToString();

                    SqlCommand tCommand = new SqlCommand(string.Format("select CityId from City where Name = '{0}' and StateId = 35", r["City"]), connection);
                    SqlDataReader tReader = tCommand.ExecuteReader();
                    tReader.Read();
                    newr[1] = tReader[0];
                    tReader.Close();

                    tCommand = new SqlCommand(string.Format("select StateId from State where ShortName = '{0}'", r["State"]), connection);
                    tReader = tCommand.ExecuteReader();
                    tReader.Read();
                    newr[1] = tReader[0];
                    tReader.Close();

                    newT.Rows.Add(newr);
                }
                connection.Close();
            }

            newT.ToCsv("c:\\temp\\ny_zc.csv");
        }

        private static void GetCityId(string connStr)
        {
            var parser = new GenericParserAdapter("c:\\temp\\ny.csv");
            parser.FirstRowHasHeader = true;
            DataTable t = parser.GetDataTable();

            DataTable newT = new DataTable();
            newT.Columns.Add("ZipCode");
            newT.Columns.Add("CityId");

            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();
                foreach (DataRow r in t.Rows)
                {
                    DataRow newr = newT.NewRow();
                    SqlCommand tCommand = new SqlCommand(string.Format("select CityId from City where Name = '{0}' and StateId = 35", r["City"]), connection);
                    SqlDataReader tReader = tCommand.ExecuteReader();
                    tReader.Read();
                    newr[0] = r[0];
                    newr[1] = tReader[0];
                    tReader.Close();
                    newT.Rows.Add(newr);
                }
                connection.Close();
            }

            newT.ToCsv("c:\\temp\\ny_zc.csv");
        }
        #endregion GetCityId

    }
}

