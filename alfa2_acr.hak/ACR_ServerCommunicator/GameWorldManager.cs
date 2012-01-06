﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ALFA;
using CLRScriptFramework;

namespace ACR_ServerCommunicator
{
    /// <summary>
    /// This object (which is a singleton) manages the local server's view of
    /// the game world wide state (data for all servers and players is locally
    /// stored here).
    /// 
    /// State elements are, generally, lazily added to the game world manager's
    /// state cache.  Only when a character, player, or server has actually
    /// been observed will it be added to the local cache.  This helps prevent
    /// the local cache from growing too large up front, as it does not need to
    /// have the entire database contents pre-downloaded.
    /// 
    /// As identifiers for unknown servers, players, or characters are seen,
    /// the local state cache is updated from the database.
    /// 
    /// The local state cache is also updated based on polling the online
    /// player list periodically.  In effect, the state cache serves as the
    /// current server's view of player presence across the entire game world,
    /// spanning every online server.
    /// 
    /// All internal operations on the GameWorldManager assume that the caller
    /// has provided appropriate synchronization.  Many operations may block on
    /// the first reference to a previously unresolved object identifier or
    /// object name.
    /// </summary>
    class GameWorldManager
    {

        /// <summary>
        /// Create a new GameWorldManager.
        /// </summary>
        /// <param name="LocalServerId">Supplies the server id of the local
        /// server.</param>
        public GameWorldManager(int LocalServerId)
        {
            this.LocalServerId = LocalServerId;
            this.PauseUpdates = false;

            EventQueue = new GameEventQueue(this);
            QueryDispatchThread = new Thread(QueryDispatchThreadRoutine);

            QueryDispatchThread.Start();
        }

        /// <summary>
        /// Reference the data for a character by the character name.  If the
        /// data was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="CharacterName">Supplies the object name.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GameCharacter ReferenceCharacterByName(string CharacterName)
        {
            //
            // Check if the object is already known first.
            //

            GameCharacter Character = (from C in Characters
                                       where C.CharacterName == CharacterName
                                       select C).FirstOrDefault();

            if (Character != null)
                return Character;

            //
            // Need to pull the data from the database.
            //

            int ServerId;

            Database.ACR_SQLQuery(String.Format(
                "SELECT `ID`, `PlayerID`, `IsOnline`, `ServerID`, `Name` FROM `characters` WHERE `Name` = '{0}'",
                Database.ACR_SQLEncodeSpecialChars(CharacterName)));

            if (!Database.ACR_SQLFetch())
                return null;

            Character = new GameCharacter(this);

            Character.CharacterId = Convert.ToInt32(Database.ACR_SQLGetData(0));
            Character.PlayerId = Convert.ToInt32(Database.ACR_SQLGetData(1));
            Character.Online = Convert.ToInt32(Database.ACR_SQLGetData(2)) != 0;
            ServerId = Convert.ToInt32(Database.ACR_SQLGetData(3));
            Character.CharacterName = Database.ACR_SQLGetData(4);

            InsertNewCharacter(Character, ServerId);

            return Character;
        }

        /// <summary>
        /// Reference the data for a character by the character id.  If the
        /// data was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="CharacterId">Supplies the object id.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GameCharacter ReferenceCharacterById(int CharacterId)
        {
            //
            // Check if the object is already known first.
            //

            GameCharacter Character = (from C in Characters
                                       where C.CharacterId == CharacterId
                                       select C).FirstOrDefault();

            if (Character != null)
                return Character;

            //
            // Need to pull the data from the database.
            //

            int ServerId;

            Database.ACR_SQLQuery(String.Format(
                "SELECT `Name`, `PlayerID`, `IsOnline`, `ServerID` FROM `characters` WHERE `ID` = {0}",
                CharacterId));

            if (!Database.ACR_SQLFetch())
                return null;

            Character = new GameCharacter(this);

            Character.CharacterName = Database.ACR_SQLGetData(0);
            Character.CharacterId = CharacterId;
            Character.PlayerId = Convert.ToInt32(Database.ACR_SQLGetData(1));
            Character.Online = Convert.ToInt32(Database.ACR_SQLGetData(2)) != 0;
            ServerId = Convert.ToInt32(Database.ACR_SQLGetData(3));

            InsertNewCharacter(Character, ServerId);

            return Character;
        }

        /// <summary>
        /// Reference the data for a player by the player name.  If the data
        /// was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="PlayerName">Supplies the object name.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GamePlayer ReferencePlayerByName(string PlayerName)
        {
            //
            // Check if the object is already known first.
            //

            GamePlayer Player = (from P in Players
                                 where P.PlayerName == PlayerName
                                 select P).FirstOrDefault();

            if (Player != null)
                return Player;

            //
            // Need to pull the data from the database.
            //

            Database.ACR_SQLQuery(String.Format(
                "SELECT `ID`, `IsDM`, `Name` FROM `players` WHERE `Name` = '{0}'",
                Database.ACR_SQLEncodeSpecialChars(PlayerName)));

            if (!Database.ACR_SQLFetch())
                return null;

            Player = new GamePlayer(this);

            Player.PlayerId = Convert.ToInt32(Database.ACR_SQLGetData(0));
            Player.IsDM = Convert.ToInt32(Database.ACR_SQLGetData(1)) != 0;
            Player.PlayerName = Database.ACR_SQLGetData(2);

            InsertNewPlayer(Player);

            return Player;
        }

        /// <summary>
        /// Reference the data for a player by the player id.  If the data
        /// was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="PlayerId">Supplies the object id.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GamePlayer ReferencePlayerById(int PlayerId)
        {
            //
            // Check if the object is already known first.
            //

            GamePlayer Player = (from P in Players
                                 where P.PlayerId == PlayerId
                                 select P).FirstOrDefault();

            if (Player != null)
                return Player;

            //
            // Need to pull the data from the database.
            //

            Database.ACR_SQLQuery(String.Format(
                "SELECT `Name`, `IsDM` FROM `players` WHERE `ID` = {0}",
                PlayerId));

            if (!Database.ACR_SQLFetch())
                return null;

            Player = new GamePlayer(this);

            Player.PlayerName = Database.ACR_SQLGetData(0);
            Player.PlayerId = PlayerId;
            Player.IsDM = Convert.ToInt32(Database.ACR_SQLGetData(1)) != 0;

            InsertNewPlayer(Player);

            return Player;
        }

        /// <summary>
        /// Reference the data for a server by the server name.  If the data
        /// was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="ServerName">Supplies the object name.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GameServer ReferenceServerByName(string ServerName)
        {
            //
            // Check if the object is already known first.
            //

            GameServer Server = (from S in Servers
                                 where S.ServerName == ServerName
                                 select S).FirstOrDefault();

            if (Server != null)
                return Server;

            //
            // Need to pull the data from the database.
            //

            Database.ACR_SQLQuery(String.Format(
                "SELECT `ID`, `IPAddress`, `Name` FROM `servers` WHERE `Name` = '{0}'",
                Database.ACR_SQLEncodeSpecialChars(ServerName)));

            if (!Database.ACR_SQLFetch())
                return null;

            Server = new GameServer(this);

            Server.ServerId = Convert.ToInt32(Database.ACR_SQLGetData(0));
            Server.SetHostnameAndPort(Database.ACR_SQLGetData(1));
            Server.ServerName = Database.ACR_SQLGetData(2);

            InsertNewServer(Server);

            return Server;
        }

        /// <summary>
        /// Reference the data for a server by the server id.  If the data
        /// was not yet available, it is retrieved from the database.
        /// </summary>
        /// <param name="ServerId">Supplies the object id.</param>
        /// <returns>The object data is returned, else null if the object did
        /// not exist.</returns>
        public GameServer ReferenceServerById(int ServerId)
        {
            //
            // Check if the object is already known first.
            //

            GameServer Server = (from S in Servers
                                 where S.ServerId == ServerId
                                 select S).FirstOrDefault();

            if (Server != null)
                return Server;

            //
            // Need to pull the data from the database.
            //

            Database.ACR_SQLQuery(String.Format(
                "SELECT `Name`, `IPAddress` FROM `servers` WHERE `ID` = {0}",
                ServerId));

            if (!Database.ACR_SQLFetch())
                return null;

            Server = new GameServer(this);

            Server.ServerName = Database.ACR_SQLGetData(0);
            Server.ServerId = ServerId;
            Server.SetHostnameAndPort(Database.ACR_SQLGetData(1));

            InsertNewServer(Server);

            return Server;
        }

        /// <summary>
        /// This property returns the database object that other objects may
        /// use.
        /// </summary>
        public ALFA.IALFADatabase Database
        {
            get { return DatabaseLink; }
        }

        /// <summary>
        /// This property returns the list of active servers.
        /// </summary>
        public IEnumerable<GameServer> Servers
        {
            get { return ServerList; }
        }

        /// <summary>
        /// This property returns the list of active players.
        /// </summary>
        public IEnumerable<GamePlayer> Players
        {
            get { return PlayerList; }
        }

        /// <summary>
        /// This property returns the list of active characters.
        /// </summary>
        public IEnumerable<GameCharacter> Characters
        {
            get { return CharacterList; }
        }

        /// <summary>
        /// This property returns the list of all online characters across all
        /// known servers.
        /// </summary>
        public IEnumerable<GameCharacter> OnlineCharacters
        {
            get { return OnlineCharacterList; }
        }

        /// <summary>
        /// This property indicates whether updates to the game state by the
        /// query thread should be paused.  It may be used to temporarly halt
        /// updates for debugging or diagnostic purposes.
        /// </summary>
        public bool PauseUpdates { get; set; }
        
        /// <summary>
        /// If true, debug mode is enabled (logs events to server log for
        /// troubleshooting presence).
        /// </summary>
        public const bool DEBUG_MODE = true;

        /// <summary>
        /// Run the event queue down.  All events in the queue are given a
        /// chance to run.
        /// </summary>
        /// <param name="Script">Supplies the script object.</param>
        /// <param name="Database">Supplies the database connection.</param>
        public void RunQueue(CLRScriptBase Script, ALFA.Database Database)
        {
            EventQueue.RunQueue(Script, Database);
        }

        /// <summary>
        /// Check whether the event queue is empty.
        /// </summary>
        /// <returns>True if the queue is empty.</returns>
        public bool IsEventQueueEmpty()
        {
            return EventQueue.Empty();
        }


        /// <summary>
        /// This method is called when a character is discovered to have come
        /// online.  The character is inserted already.
        /// </summary>
        /// <param name="Character">Supplies the character that is now
        /// considered to be online.</param>
        private void OnCharacterJoin(GameCharacter Character)
        {
            EventQueue.EnqueueEvent(new CharacterJoinEvent(Character, Character.Player.IsDM, Character.Server));
        }

        /// <summary>
        /// This method is called when a character is discovered to have gone
        /// offline.
        /// </summary>
        /// <param name="Character">Supplies the character that is now
        /// considered to be offline.</param>
        private void OnCharacterPart(GameCharacter Character)
        {
            EventQueue.EnqueueEvent(new CharacterPartEvent(Character, Character.Player.IsDM, Character.Server));
        }

        /// <summary>
        /// This method is called when a server is discovered to have come
        /// online.  The server is inserted already.
        /// </summary>
        /// <param name="Server">Supplies the server that is now considered to
        /// be online.</param>
        private void OnServerJoin(GameServer Server)
        {
            EventQueue.EnqueueEvent(new ServerJoinEvent(Server));
        }

        /// <summary>
        /// This method is called when a server is discovered to have gone
        /// offline.
        /// </summary>
        /// <param name="Server">Suplies the server that is now considered to
        /// be offline.</param>
        private void OnServerPart(GameServer Server)
        {
            EventQueue.EnqueueEvent(new ServerPartEvent(Server));
        }

        /// <summary>
        /// This method is called when a chat tell IPC event is received.
        /// </summary>
        /// <param name="Sender">Supplies the sender.</param>
        /// <param name="Recipient">Supplies the recipient.</param>
        /// <param name="Message">Supplies the message text.</param>
        private void OnChatTell(GameCharacter Sender, GameCharacter Recipient, string Message)
        {
            EventQueue.EnqueueEvent(new ChatTellEvent(Sender, Recipient, Message));
        }

        /// <summary>
        /// This method is called when an unsupported IPC event code is
        /// received.
        /// </summary>
        /// <param name="RecordId">Supplies the associated record ID.</param>
        /// <param name="P0">Supplies the P0 parameter.</param>
        /// <param name="P1">Supplies the P1 parameter.</param>
        /// <param name="P2">Supplies the P2 parameter.</param>
        /// <param name="EventType">Supplies the IPC event type.</param>
        /// <param name="P3">Supplies the P3 parameter.</param>
        private void OnUnsupportedIPCEventType(int RecordId, int P0, int P1, int P2, int EventType, string P3)
        {
            EventQueue.EnqueueEvent(new UnsupportedIPCRequestEvent(RecordId, P0, P1, P2, EventType, P3));
        }

        /// <summary>
        /// This method is called when a player has had all of its data loaded
        /// from the database.  The player is inserted already.
        /// </summary>
        /// <param name="Player">Supplies the player object hwich has been
        /// loaded.</param>
        private void OnPlayerLoaded(GamePlayer Player)
        {
        }

        /// <summary>
        /// This method is called when a server has had all of its data loaded
        /// from the database.  The server is inserted already.
        /// </summary>
        /// <param name="Server">Supplies the server object hwich has been
        /// loaded.</param>
        private void OnServerLoaded(GameServer Server)
        {
            if (Server.Online)
                EventQueue.EnqueueEvent(new ServerJoinEvent(Server));
        }

        /// <summary>
        /// This function inserts a character into the various character lists
        /// and issues the character join event.
        /// </summary>
        /// <param name="Character">Supplies the character object to insert.
        /// </param>
        /// <param name="ServerId">Supplies the server id that the player is
        /// logged on to (only meaningful if the character is online).</param>
        private void InsertNewCharacter(GameCharacter Character, int ServerId)
        {
            GameServer Server;

            Character.Player = ReferencePlayerById(Character.PlayerId);

            if (Character.Player == null)
                throw new ApplicationException(String.Format("Character {0} references invalid player id {1}", Character.CharacterId, Character.PlayerId));

            Character.Player.Characters.Add(Character);

            try
            {
                CharacterList.Add(Character);

                try
                {
                    if (Character.Online)
                    {
                        Server = ReferenceServerById(ServerId);

                        if (Server == null)
                            throw new ApplicationException(String.Format("Character {0} is online but references invalid server id {1}", Character.CharacterId, ServerId));

                        //
                        // If the character is coming online, but its associated server is
                        // not actually online, then mark the character as offline.
                        //

                        if (Character.Online && !Character.Server.Online)
                        {
                            Character.Online = false;
                            return;
                        }

                        //
                        // Mark the character as visited so that if we come in
                        // on the main thread during the middle of a character
                        // synchronization cycle, we won't immediate offline
                        // the character.
                        //

                        Character.Visited = true;
                        Character.Server = Server;
                        Character.Server.Characters.Add(Character);

                        try
                        {
                            OnlineCharacterList.Add(Character);

                            try
                            {
                                Character.Player.UpdateOnlineCharacter();
                                OnCharacterJoin(Character);
                            }
                            catch
                            {
                                OnlineCharacterList.Remove(Character);
                                throw;
                            }
                        }
                        catch
                        {
                            Character.Server.Characters.Remove(Character);
                            throw;
                        }
                    }
                }
                catch
                {
                    CharacterList.Remove(Character);
                    throw;
                }
            }
            catch
            {
                Character.Player.Characters.Remove(Character);
                Character.Player.UpdateOnlineCharacter();
                throw;
            }
        }

        /// <summary>
        /// This function inserts a player into the various player lists and
        /// issues the player load event.
        /// </summary>
        /// <param name="Player">Supplies the player object to insert.
        /// </param>
        private void InsertNewPlayer(GamePlayer Player)
        {
            PlayerList.Add(Player);
            OnPlayerLoaded(Player);
        }

        /// <summary>
        /// This function inserts a server into the various server lists and
        /// issues the server load event.
        /// </summary>
        /// <param name="Server">Supplies the server object to insert.
        /// </param>
        private void InsertNewServer(GameServer Server)
        {
            //
            // Mark the server as visited so that if we come in on the main
            // thread during the middle of a server synchronization cycle, we
            // won't immediate offline the server.
            //

            Server.Visited = true;
            Server.RefreshOnlineStatus();

            ServerList.Add(Server);
            OnServerLoaded(Server);
        }

        /// <summary>
        /// This thread routine periodically queries the database for new
        /// events and updates the local state cache as appropriate.  It also
        /// enqueues game events to the game event queue as required.
        /// </summary>
        private void QueryDispatchThreadRoutine()
        {
            for (; ; )
            {
                //
                // Run the query cycle and log any exceptions.
                //

                try
                {
                    if (!PauseUpdates)
                        RunQueryCycle();
                }
                catch (Exception e)
                {
                    //
                    // Try and log the exception.  If that fails, don't take
                    // any other actions.

                    try
                    {
                        EventQueue.EnqueueEvent(new DiagnosticLogEvent(String.Format(
                            "GameWorldManager.QueryDispatchThreadRoutine: Exception {0} running query cycle.", e)));
                    }
                    catch
                    {

                    }
                }

                Thread.Sleep(DATABASE_POLLING_INTERVAL);
            }
        }

        /// <summary>
        /// This method polls the database for changes and updates the game
        /// world state as appropriate.
        /// </summary>
        private void RunQueryCycle()
        {
            DatabasePollCycle += 1;

            //
            // Always synchronize the character list.
            //

            SynchronizeOnlineCharacters();

            //
            // Always synchronize the IPC queue.
            //

            SynchronizeIPCEventQueue();

            //
            // Every POLLING_CYCLES_TO_SERVER_SYNC cycles, update the online
            // status of known servers.
            //

            if (DatabasePollCycle % POLLING_CYCLES_TO_SERVER_SYNC == 0)
                SynchronizeOnlineServers();
        }

        /// <summary>
        /// This method synchronizes the online character list with the central
        /// database.  Character join or part events are generated, as is
        /// appropriate.
        /// </summary>
        private void SynchronizeOnlineCharacters()
        {
            //
            // Query the current player list and synchronize with our internal
            // state.
            //
            // There are several steps here:
            //
            // 1) Mark all online characters as "not visited".
            // 2) Retrieve the character list from the database.  For each
            //    character that wasn't already marked as online, or which has
            //    changed servers, update the state and fire the character join
            //    event.  Also, flag any character that is in the list from the
            //    database as "visited".
            // 3) Sweep the online character list for characters that are still
            //    flagged as "not visited".  These characters are those that
            //    have gone offline in the interim time and which need to have
            //    the character part event fired.
            //

            //
            // First - query the database.  This query returns a list of all
            // online characters from all servers that have checked in in the
            // past ten minutes.  We treat any server that has not checked in
            // within at least that long as having no online players.
            //

            Database.ACR_SQLQuery(
                "SELECT " +
                    "`characters`.`ID` AS character_id, " +
                    "`players`.`IsDM` AS character_is_dm, " +
                    "`servers`.`ID` AS character_server_id " +
                "FROM `characters` " +
                "INNER JOIN `players` ON `players`.`ID` = `characters`.`PlayerID` " +
                "INNER JOIN `servers` ON `servers`.`ID` = `characters`.`ServerID` " +
                "INNER JOIN `pwdata` ON `pwdata`.`Name` = `servers`.`Name` " +
                "WHERE `characters`.`IsOnline` = 1 " +
                "AND pwdata.`Key` = 'ACR_TIME_SERVERTIME' " +
                "AND pwdata.`Last` >= DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 10 MINUTE) "
                );

            //
            // Clear visited.
            //
            // N.B.  The Visited flag is only managed on the query thread or
            //       prior to list insertion during initial character creation.
            //
            //       Thus, while we must lock to protect the integrity of the
            //       list, there's no need to worry about another thread
            //       altering the Visited state after we drop the lock.
            //

            lock (this)
            {
                foreach (GameCharacter Character in OnlineCharacters)
                {
                    Character.Visited = false;
                }
            }

            //
            // Read each database row, then take the lock and update entries.
            //

            while (Database.ACR_SQLFetch())
            {
                int CharacterId = Convert.ToInt32(Database.ACR_SQLGetData(0));
                bool IsDM = Convert.ToInt32(Database.ACR_SQLGetData(1)) != 0;
                int ServerId = Convert.ToInt32(Database.ACR_SQLGetData(2));

                lock (this)
                {
                    GameCharacter Character = ReferenceCharacterById(CharacterId);
                    GameServer Server = ReferenceServerById(ServerId);

                    //
                    // Update the DM state of the character.
                    //

                    Character.Visited = true;
                    Character.Player.IsDM = IsDM;

                    if (Character.Server == Server)
                        continue;

                    //
                    // The character changed servers or came online from not
                    // being online, send the appropriate part and join events.
                    //

                    if (Character.Server != null && Character.Online)
                        OnCharacterPart(Character);

                    //
                    // If the user's server is still marked as offline, mark it
                    // as online now.  It has to have checked in for it to have
                    // been returned in the query as it is.
                    //

                    if (!Server.Online)
                    {
                        Server.Online = true;
                        OnServerJoin(Server);
                    }

                    Character.Server = Server;
                    Character.Online = true;
                    OnCharacterJoin(Character);
                }
            }

            //
            // Sweep offline characters.
            //

            lock (this)
            {
                var NowOfflineCharacters = (from C in OnlineCharacters
                                            where C.Visited == false
                                            select C);

                foreach (GameCharacter Character in NowOfflineCharacters)
                {
                    Character.Online = false;
                    OnlineCharacterList.Remove(Character);
                    OnCharacterPart(Character);
                }
            }
        }

        /// <summary>
        /// This method synchronizes the online status of the server list.
        /// </summary>
        private void SynchronizeOnlineServers()
        {
            //
            // Query the current server list and synchronize with our internal
            // state.
            //
            // There are several steps here:
            //
            // 1) Mark all online servers as "not visited".
            // 2) Retrieve the server list from the database.  For each server
            //    that wasn't already marked as online, mark it as online.
            //    Also, flag any server that is in the list from the database
            //    as "visited".
            // 3) Sweep the online server list for servers that are still
            //    flagged as "not visited".  These servers are those that have
            //    gone offline in the interim time.  Note that we don't yet
            //    update the player list, as that is handled by the player list
            //    synchronization tep.
            //

            //
            // First - query the database.  This query returns a list of all
            // servers that have checked in during the past ten minutes.  We
            // treat any server that has not checked in within at least that
            // long as being offline.
            //

            Database.ACR_SQLQuery(
                "SELECT " +
                    "`servers`.`ID` AS server_id " +
                "FROM `servers` " +
                "INNER JOIN `pwdata` ON `pwdata`.`Name` = `servers`.`Name` " +
                "WHERE pwdata.`Key` = 'ACR_TIME_SERVERTIME' " +
                "AND pwdata.`Last` >= DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 10 MINUTE) "
                );

            //
            // Clear visited.
            //
            // N.B.  The Visited flag is only managed on the query thread or
            //       prior to list insertion during initial server creation.
            //
            //       Thus, while we must lock to protect the integrity of the
            //       list, there's no need to worry about another thread
            //       altering the Visited state after we drop the lock.
            //

            lock (this)
            {
                foreach (GameServer Server in Servers)
                {
                    Server.Visited = false;
                }
            }

            //
            // Read each database row, then take the lock and update entries.
            //

            while (Database.ACR_SQLFetch())
            {
                int ServerId = Convert.ToInt32(Database.ACR_SQLGetData(0));

                lock (this)
                {
                    GameServer Server = ReferenceServerById(ServerId);

                    Server.Visited = true;

                    if (!Server.Online)
                    {
                        Server.Online = true;
                        OnServerJoin(Server);
                    }
                }
            }

            //
            // Sweep offline servers.
            //

            lock (this)
            {
                var NowOfflineServers = (from S in Servers
                                         where S.Visited == false &&
                                         S.Online == true
                                         select S);

                foreach (GameServer Server in NowOfflineServers)
                {
                    Server.Online = false;
                    OnServerPart(Server);
                }
            }

        }

        /// <summary>
        /// This method synchronizes the IPC event queue for the server.
        /// </summary>
        private void SynchronizeIPCEventQueue()
        {
            int HighestRecordId = 0;

            //
            // Retrieve any new IPC events from the database.
            //

            Database.ACR_SQLQuery(String.Format(
                "SELECT " +
                    "`server_ipc_events`.`ID` as record_id, " +
                    "`server_ipc_events`.`SourcePlayerID` as source_player_id, " +
                    "`server_ipc_events`.`SourceServerID` as source_server_id, " +
                    "`server_ipc_events`.`DestinationPlayerID` as destination_player_id, " +
                    "`server_ipc_events`.`DestinationServerID` as destination_server_id, " +
                    "`server_ipc_events`.`EventType` as event_type, " +
                    "`server_ipc_events`.`EventText` as event_text " +
                "FROM `server_ipc_events` " +
                "WHERE destination_server_id = {0} " +
                "GROUP BY record_id " +
                "ORDER BY record_id ",
                LocalServerId
                ));

            //
            // Dispatch each of them.
            //

            try
            {
               while (Database.ACR_SQLFetch())
               {
                   int RecordId = Convert.ToInt32(Database.ACR_SQLGetData(0));
                   int SourcePlayerId = Convert.ToInt32(Database.ACR_SQLGetData(1));
                   int SourceServerId = Convert.ToInt32(Database.ACR_SQLGetData(2));
                   int DestinationPlayerId = Convert.ToInt32(Database.ACR_SQLGetData(3));
                   int DestinationServerId = Convert.ToInt32(Database.ACR_SQLGetData(4));
                   int EventType = Convert.ToInt32(Database.ACR_SQLGetData(5));
                   string EventText = Database.ACR_SQLGetData(6);

                   HighestRecordId = RecordId;

                   switch (EventType)
                   {

                       case ACR_SERVER_IPC_EVENT_CHAT_TELL:
                           lock (this)
                           {
                               GamePlayer SenderPlayer = ReferencePlayerById(SourcePlayerId);
                               GamePlayer RecipientPlayer = ReferencePlayerById(DestinationPlayerId);

                               if (SenderPlayer == null || RecipientPlayer == null)
                               {
                                   EventQueue.EnqueueEvent(new DiagnosticLogEvent(String.Format(
                                       "GameWorldManager.SynchronizeIPCEventQueue: Source {0} or destination {1} player IDs invalid for ACR_SERVER_IPC_EVENT_CHAT_TELL.", SourcePlayerId, DestinationPlayerId)));
                                   continue;

                               }

                               GameCharacter SenderCharacter = SenderPlayer.GetOnlineCharacter();
                               GameCharacter RecipientCharacter = RecipientPlayer.GetOnlineCharacter();

                               if (SenderCharacter == null || RecipientCharacter == null)
                               {
                                   EventQueue.EnqueueEvent(new DiagnosticLogEvent(String.Format(
                                       "GameWorldManager.SynchronizeIPCEventQueue: Source {0} or destination {1} player has already gone offline for ACR_SERVER_IPC_EVENT_CHAT_TELL.", SourcePlayerId, DestinationPlayerId)));
                                   continue;
                               }

                               OnChatTell(SenderCharacter, RecipientCharacter, EventText);
                           }
                           break;

                       default:
                           lock (this)
                           {
                               OnUnsupportedIPCEventType(RecordId, SourcePlayerId, SourceServerId, DestinationPlayerId, EventType, EventText);
                           }
                           break;

                   }
               }
            }
            finally
            {
               //
               // Now delete all of the records that we processed.
               //

               if (HighestRecordId != 0)
               {
                   Database.ACR_SQLQuery(String.Format(
                       "DELETE FROM `server_ipc_events` WHERE `DestinationServerID` = {0} AND `ID` < {1}",
                       LocalServerId,
                       HighestRecordId));
               }
            }
        }

        /// <summary>
        /// Chat tell IPC events use this event type.  For this event, there
        /// are five parameters.  The source and destination IDs represent the
        /// routing information for the chat tell originator and destination,
        /// and the event text represents the chat text to deliver.
        /// </summary>
        public const int ACR_SERVER_IPC_EVENT_CHAT_TELL = 0;


        /// <summary>
        /// The count, in milliseconds, between database polling attempts in
        /// the context of the query dispatch thread.
        /// </summary>
        private const int DATABASE_POLLING_INTERVAL = 1000;

        /// <summary>
        /// The number of polling cycles between a server online status
        /// synchronization attempt.
        /// </summary>
        private const int POLLING_CYCLES_TO_SERVER_SYNC = 60;


        /// <summary>
        /// The database connection object.
        /// </summary>
        private ALFA.MySQLDatabase DatabaseLink = new ALFA.MySQLDatabase();

        /// <summary>
        /// The list of known servers is stored here.
        /// </summary>
        private List<GameServer> ServerList = new List<GameServer>();

        /// <summary>
        /// The list of known players is stored here.  The list may be
        /// incomplete and is expanded on demand.
        /// </summary>
        private List<GamePlayer> PlayerList = new List<GamePlayer>();

        /// <summary>
        /// The list of known characters is stored here.  The list may be
        /// incomplete and is expanded on demand.
        /// </summary>
        private List<GameCharacter> CharacterList = new List<GameCharacter>();

        /// <summary>
        /// The list of characters that are online is stored here.
        /// </summary>
        private List<GameCharacter> OnlineCharacterList = new List<GameCharacter>();

        /// <summary>
        /// The event queue for pending game events that require service from
        /// within an in-script is stored here.
        /// </summary>
        private GameEventQueue EventQueue = null;

        /// <summary>
        /// The thread object for the query dispatch thread is stored here.
        /// </summary>
        private Thread QueryDispatchThread = null;

        /// <summary>
        /// The current polling cycle for the database is stored here.
        /// </summary>
        private int DatabasePollCycle = 0;

        /// <summary>
        /// The server id of the local (current) server is stored here.
        /// </summary>
        private int LocalServerId = 0;
    }
}
