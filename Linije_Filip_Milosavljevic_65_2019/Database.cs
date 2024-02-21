using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linije_Filip_Milosavljevic_65_2019
{
    public class Database : IDatabase
    {
        private SqlConnection connection = ConnectionDB.Connect();
        public Score GetBestScore()
        {
            Score bestScore = null;

            try
            {
                connection.Open();

                string query = @"SELECT id, time, score FROM user_score WHERE score = (SELECT MAX(score) FROM user_score);";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bestScore = new Score
                            {
                                id = int.Parse(reader["id"].ToString()),
                                time = int.Parse(reader["time"].ToString()),
                                score = int.Parse(reader["score"].ToString())
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return bestScore;
        }


        public void InsertScore(int score, int time)
        {
            try
            {
                connection.Open();

                string query = @"INSERT INTO user_score (time, score) VALUES (@time, @score);";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@time", time);
                    cmd.Parameters.AddWithValue("@score", score);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                connection.Close();
            }
        }

    }
}
