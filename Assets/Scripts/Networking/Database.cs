using UnityEngine;
using Mirror;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;

/// <summary>
/// Singleton class for connecting to and making use of the database. 
/// </summary>
public static class Database
{

    #region Fields
    private static string dbUri = "URI=file:Database.sqlite";
    [Tooltip("Connection to the sqlite database")]
    private static IDbConnection conn;
    public static CVWNetworkManager manager;
    #endregion

    /// <summary>
    /// Container for account data. Should probably make the fields secure instead of just public. 
    /// </summary>
    public struct Account
    {
        public string name;
        public DateTime created;
        public DateTime lastlogin;
        public bool banned;
        public bool isCounselor;
    }

    #region Initialize

    /// <summary>
    /// Initiatite connection with the server's database. The database should be an sqlite file in the project files. 
    /// </summary>
    public static void Connect()
    {
        conn = new SqliteConnection(dbUri);

        if (conn == null)
        {
            Debug.LogError("Error connecting to database");
            return;
        }
        else
        {
            Debug.Log("Connected to database");
        }

        SetupTables();
    }

    /// <summary>
    /// Sets up database tables in case they are not present or incorrectly configured.
    /// </summary>
    private static void SetupTables()
    {
        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "CREATE TABLE IF NOT EXISTS accounts (name STRING, password STRING PRIMARY KEY, created VARCHAR, lastlogin VARCHAR, banned INTEGER, isCounselor BOOLEAN )";
        dbCommandReadValues.ExecuteNonQuery();
        dbCommandReadValues.CommandText = "CREATE TABLE IF NOT EXISTS characters (name STRING, account STRING PRIMARY KEY, x INTEGER, y INTEGER, z INTEGER, online VARCHAR, lastsaved VARCHAR )";
        dbCommandReadValues.ExecuteNonQuery();

        conn.Close();
    }

    #endregion

    #region Account

    /// <summary>
    /// Attempt to login to account 'username' by entering 'password'
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static bool LogOn(string username, string password, bool debug)
    {

        bool exists = AccountExists(username);
        if (exists)
        {
            //Password checking will not actually stop logon atm, it will just print a message on the host console
            //if (!debug)
            //{
                bool pass = CheckPassword(username, password);
                if (pass == false) return false;
            //}
            //Should really add some kind of clientside ui popup that says their password is bad

            //This doesn't return anything for now because it doesn't have to
            //GetSpecificAccount(username);

            //remember to update lastLogin field
        }
        else
        {
            CreateAccount(username, password);
        }

        Debug.Log("Logging On " + username);
        return true;
    }

    /// <summary>
    /// Adds or overwrites character data in the database
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static void CreateAccount(string username, string password) //, bool useTransaction = true
    {

        conn.Open();

        IDbCommand dbCommandInsertValue = conn.CreateCommand();
        dbCommandInsertValue.CommandText = "INSERT OR REPLACE INTO accounts (name, password, created, lastlogin, banned, isCounselor, isAdmin) VALUES " +
            "('" + username + "', '" + password + "', '" + DateTime.Now + "', '" + DateTime.Now + "', '" + false + "', '" + false + "', '" + false + "')";
        dbCommandInsertValue.ExecuteNonQuery();

        conn.Close();

    }

    /// <summary>
    /// Check if an account with the given username exists. 
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static bool AccountExists(string username)
    {
        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name FROM accounts";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            var namesPool = dataReader.GetString(0);

            //is iterating to the new username
            if (namesPool == username)
            {
                conn.Close();
                return true;
            }
        }

        conn.Close();

        return false;
    }

    /// <summary>
    /// Checks that a user entered the correct password for their account
    /// </summary>
    /// <returns></returns>
    private static bool CheckPassword(string username, string password)
    {
        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name, password FROM accounts";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            //TODO: Debug, this probably won't work

            var namesPool = dataReader.GetString(0);
            var passPool = dataReader.GetString(1);

            //is iterating to the new username
            if (namesPool == username && passPool == password)
            {
                conn.Close();
                return true;
            }
        }

        conn.Close();

        return false;
    }

    /// <summary>
    /// Returns a List of all accounts on the server. This method is for the host or those with auth only
    /// </summary>
    public static List<string> GetAccounts()
    {
        List<string> accounts = new List<string>();

        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name FROM accounts";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            var namesPool = dataReader.GetString(0);
            accounts.Add(namesPool);
        }

        conn.Close();

        return accounts;
    }

    // WIP NEEDS FINISHED will currently throw error
    // Don't need it yet so is on the backburner
    /// <summary>
    /// Returns a List of all accounts on the server. This method is for the host or those with auth only
    /// </summary>
    public static Account GetSpecificAccount(string name)
    {
        Account acc = new Account();
        int iterator = 1;

        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name FROM accounts WHERE name=" + name;

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            var read = dataReader.GetValue(0);

            switch (iterator)
            {
                /*case 1:
                    acc.name = read;
                    break;
                    //skip 2 because that is password, not needed
                case 3:
                    acc.created = read;
                    break;
                case 4:
                    acc.lastlogin = read;
                    break;
                case 5:
                    acc.banned = read;
                    break;
                case 6:
                    acc.isCounselor = read;
                    break;*/
            }
            iterator++;
        }

        conn.Close();

        return acc;
    }

    #endregion

    #region Character

    /// <summary>
    /// Needs to be finished. Will load a character if it already exists in the database. If not, will make one. 
    /// </summary>
    /// <param name="player"></param>
    public static void GetCharacter(Player player)
    {
        Debug.Log("Get character: " + player.name);
        if (CharacterExists(player.name))
        {
            LoadCharacter(player);
        }
        else
        {
            CharacterSave(player);
        }
    }

    /// <summary>
    /// Check if the database already contains a given character based on their name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool CharacterExists(string name)
    {
        bool validName = false;

        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name FROM characters";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            var namesPool = dataReader.GetString(0);

            //is iterating to the new username
            if (namesPool == name)
            {
                //Debug.LogError("Character exists");
                conn.Close();
                return false;
            }

        }

        conn.Close();

        return validName;
    }

    /// <summary>
    /// To be implemented. Get and return the given character.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private static Player LoadCharacter(Player player)
    {

        return null;
    }

    /// <summary>
    /// Adds or overwrites character data in the database.
    /// </summary>
    /// <param name="player"></param>
    public static void CharacterSave(Player player) //, bool useTransaction = true
    {
        Debug.Log("Saving character " + player.name + " to db");

        conn.Open();

        IDbCommand dbCommandInsertValue = conn.CreateCommand();
        dbCommandInsertValue.CommandText = "INSERT OR REPLACE INTO characters (name, account, lastSaved) VALUES " +
            "('" + player.name + "', '" + player.GetAccount() + "', '" + DateTime.UtcNow + "')";
        dbCommandInsertValue.ExecuteNonQuery();

        conn.Close();

    }

    /// <summary>
    /// To be implemented. Remove the given character from the database.
    /// </summary>
    /// <param name="player"></param>
    public static void DeleteCharacter(Player player)
    {
        Debug.Log("Deleting character " + player.name + " from db");

        conn.Open();

        IDbCommand dbCommandInsertValue = conn.CreateCommand();
        dbCommandInsertValue.CommandText = "DELETE FROM accounts WHERE name=" + player.name;
        dbCommandInsertValue.ExecuteNonQuery();

        conn.Close();
    }

    #endregion

    #region Roles

    /// <summary>
    /// Returns whether a given account belongs to a counselor
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static bool IsCounselor(string username)
    {
        conn.Open();

        IDbCommand dbCommandReadValues = conn.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT name, isCounselor FROM accounts";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();
        while (dataReader.Read())
        {
            //TODO: Debug, this probably won't work

            var namesPool = dataReader.GetString(0);
            bool getCouns = dataReader.GetBoolean(1);

            //is iterating to the new username
            if (namesPool == username && getCouns == true)
            {
                conn.Close();
                return true;
            }
        }

        conn.Close();

        return false;
    }

    /// <summary>
    /// Sets an account to be or not be a counselor
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isCouns"></param>
    public static void SetCounselor(Player player, bool isCouns)
    {
        conn.Open();

        IDbCommand dbCommandInsertValue = conn.CreateCommand();
        dbCommandInsertValue.CommandText = "UPDATE accounts SET isCounselor=" + isCouns + " WHERE name=" + player.name;
        dbCommandInsertValue.ExecuteNonQuery();

        conn.Close();
    }

    #endregion

}
