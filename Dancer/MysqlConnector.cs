using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace Dancer
{
    static class MysqlConnector
    {
        private static MySqlConnection connection;
        private static MySqlCommand command;
        private static MySqlDataAdapter adapter;
        public static DataSet dataset = new DataSet();
        public static void init()
        {
            string pwd = System.IO.File.ReadAllText(@"mysql.pwd");
            string conn_mes = "Data Source=47.92.75.9;Initial Catalog=dancer;User id=root;password=fakepwd;CharSet=utf8;Port=8783";
            conn_mes = conn_mes.Replace("fakepwd", pwd);
            connection = new MySqlConnection(conn_mes);
            command = new MySqlCommand("", connection);
            adapter = new MySqlDataAdapter(command);
        }
        public static int addNewSong(string music_name, string singer, string belong_to_list, string other_singer = null, string album = null, int publish_year = 0)
        {
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT count(*) FROM music WHERE music_name = '" + music_name + "' AND singer = '" + singer + "'";
            connection.Open();
            object cnt = command.ExecuteScalar();
            connection.Close();
            if(Convert.ToInt32(cnt) != 0)return -1;
            addNewList(belong_to_list);
            string _other_singer = other_singer == null ? "" : ",other_singer";
            string _album = album == null ? "" : ",album";
            string _publish_year = publish_year == 0 ? "" : ",publish_year";
            string other_singer_ = other_singer == null ? "" : decorate(other_singer);
            string album_ = album == null ? "" : decorate(album);
            string publish_year_ = publish_year == 0 ? "" : decorate(publish_year.ToString());
            string otherParamName = _other_singer + _album + _publish_year;
            string wholeParam = decorate(music_name, false) + decorate(singer) + decorate(belong_to_list) + other_singer_ + album_ + publish_year_;
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = "INSERT INTO music(music_name, singer, belong_to_list" + otherParamName + ")VALUES(" + wholeParam + ")";
            connection.Open();
            int Return = command.ExecuteNonQuery();
            connection.Close();
            return Return;
        }

        public static int addNewList(string list_name)
        {
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT count(*) FROM lists WHERE list_name = '" + list_name + "'";
            connection.Open();
            object cnt = command.ExecuteScalar();
            connection.Close();
            if (Convert.ToInt32(cnt) != 0) return -1;
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = "INSERT INTO lists(list_name)VALUES('" + list_name + "')";
            connection.Open();
            int res = command.ExecuteNonQuery();
            connection.Close();
            return res;
        }
        public static int addListeningRecord(string music_name, string singer)
        {
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = "INSERT INTO listening(music_name, singer, where_to_listen)VALUES(" + decorate(music_name,false) + decorate(singer) + decorate("PC") + ")";
            connection.Open();
            int res = command.ExecuteNonQuery();
            connection.Close();
            return res;
        }
        public static int getCurrentSong(ref string music_name, ref string singer)
        {
            command.Parameters.Clear();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "get_current_song";
            MySqlParameter _music_name = new MySqlParameter("_music_name", MySqlDbType.VarChar);
            MySqlParameter _singer = new MySqlParameter("_singer", MySqlDbType.VarChar);
            _music_name.Direction = ParameterDirection.Output;
            _singer.Direction = ParameterDirection.Output;
            command.Parameters.Add(_music_name);
            command.Parameters.Add(_singer);
            connection.Open();
            int res = command.ExecuteNonQuery();
            music_name = _music_name.Value.ToString();
            singer = _singer.Value.ToString();
            connection.Close();
            return res;
        }
        private static string decorate(string _item, bool comma = true) => (comma ? "," : "") + "'" + _item + "'";
    }
}
