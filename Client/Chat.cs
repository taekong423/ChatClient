﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;


namespace Chat_Cleint
{
    struct User
    {
        public string id;
        public string password;
        public User(string id, string pw)
        {
            this.id = id;
            this.password = pw;
        }
    }

    class Chat
    {
        Client client;
        User user;
        Message m = new Message();

        public Chat(IPAddress ip, int port)
        {
            client = new Client(ip,port);
            Main();
           
        }
        public void Main()
        {
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                      Welcome to 4:33 Chat                      |");
            Console.WriteLine("|                Type \"exit\", if you want to exit                |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. Log In                                                      |");
            Console.WriteLine("| 2. Sign In                                                     |");
            Console.WriteLine("+----------------------------------------------------------------+");

            MainHelp();

            Console.Write("> ");
            string menu = Console.ReadLine();
            menu = menu.ToLower().Replace(" ",""); 
            
            try
            {
                switch (menu)
                {
                    //Login
                    case "1": case "1login": case "login": case "log" :case "l":
                        LogIn();
                        break;

                    case "2": case "2signin": case "signin": case "sign": case "s":
                        SignUp();
                        break;

                    case "exit":
                        break;

                    default:
                        Main();
                        break;
                }
            }
            catch (FormatException fe)
            {
                MainHelp();
                Main();
            }

        }

		public void MainHelp()
		{
			Console.WriteLine("usage :");
			Console.WriteLine("       [num]");
			Console.WriteLine("       [num] [command]");
			Console.WriteLine("       [command]");
            Console.WriteLine("       [initial of command]");
			Console.WriteLine("(Commands are not case-sensitive.)");
		}


        public void SignUp()
        {
            string id;
            string pw;
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                            Sign up                             |");
            Console.WriteLine("+----------------------------------------------------------------+");

            while (true)
            {
               
                Console.Write("ID       : ");
                id = Console.ReadLine();

                if (id.Length > 10 || id.Length < 3)
                {
                    Console.WriteLine("[!]ID lenght should be between 3 and 12");
                    continue;
                }
                
                LoginRequestBody dupReqest = new LoginRequestBody(id.ToCharArray(), "-".ToCharArray());
                byte[] dupBody = m.StructureToByte(dupReqest);
                Console.WriteLine("dupBody.Length: " +dupBody.Length);
                client.Send(MessageType.Id_Dup, dupBody);

                MessageState mstate =  client.ResponseState();
                Console.WriteLine(mstate);
                if(mstate == MessageState.SUCCESS)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("state:"+mstate);
                    Console.WriteLine("[!]Duplicated ID");
                }
            }

            while (true) { 
                Console.Write("Password     : ");
                pw = Console.ReadLine();
                if (pw.Length > 10 || pw.Length < 4)
                {
                    Console.WriteLine("[!]Password lenght should be between 4 and 16");
                    continue;
                }

                Console.Write("Passeord again   : ");
                string pw2 = Console.ReadLine();

                if (pw != pw2)
                {
                    Console.WriteLine("[!] Passwords must match");
                    continue;
                }
                break;
            }
            SignupRequestBody signupReqest = new SignupRequestBody(id.ToCharArray(), pw.ToCharArray(), false);
            byte[] body = m.StructureToByte(signupReqest);
            client.Send(MessageType.Signup, body);

            MessageState state = client.ResponseState();
            if (state == MessageState.SUCCESS)
            {
                LogIn();
            }
            else
            {
                Console.WriteLine("[!]Fail to Sign up");
            }
            

        }

        public void LogIn()
        {
            string id;
            string password;
            while (true) { 
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("|                            Log In                              |");
                Console.WriteLine("+----------------------------------------------------------------+");

                Console.Write("ID       : ");
                id = Console.ReadLine();

                Console.Write("Password     : ");
                password = ReadPassword();

                LoginRequestBody logInReqest = new LoginRequestBody(id.ToCharArray(), password.ToCharArray());
                byte[] body = client.BodyStructToBytes(logInReqest);
                client.Send(MessageType.Signin, body);
                MessageState state = client.ResponseState();

                if (state == MessageState.SUCCESS)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("[!]Fail to Log in");
                }

            }
            user = new User(id, password);
            Lobby();
        }

        public string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    
        public void Lobby()
        {
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("|                             Lobby                              |");
            Console.WriteLine("+----------------------------------------------------------------+");
            Console.WriteLine("| 1. RoomList                                                    |");
            Console.WriteLine("| 2. Create Room                                                 |");
            Console.WriteLine("| 3. Enter Room                                                 |");
            Console.WriteLine("+----------------------------------------------------------------+");
            LobbyHelp();
            Console.WriteLine();
            Console.Write("> ");
            string menu = Console.ReadLine();
            menu = menu.ToLower().Replace(" ", "");

            try
            {
                switch (menu)
                {
                    //RoomList
                    case "1": case "1roomlist":
                    case "roomlist": case "room": case "list":
                    case "rl": case "r": case "l":
                        RoomRequest();
                        break;
                    case "2":
                        CreateRoom();
                        break;
                    case "3":
                        CreateRoom();
                        break;

                    case "exit":
                        client.SocketClose();
                        break;

                    default:
                        Lobby();
                        break;
                }
            }
            catch (FormatException fe)
            {
                MainHelp();
                Main();
            }
        }

        public void LobbyHelp()
        {
            Console.WriteLine("usage :");
            Console.WriteLine("       [num]");
            Console.WriteLine("       [num] [command]");
            Console.WriteLine("       [command]");
            Console.WriteLine("       [initial of command]");
            Console.WriteLine("(Commands are not case-sensitive.)");
            Console.WriteLine("[EX] 1, roomlist, room , rl, r, ...");
        }

        public void RoomRequest()
        {
            RoomRequestBody roomReqest = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = client.BodyStructToBytes(roomReqest);
            client.Send(MessageType.Signin, body);
            Header h = (Header)client.GetHeader();

            if (h.state == MessageState.SUCCESS)
            {
                byte[] b = client.Recieve(h.length);
                Console.WriteLine(b);
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
                Lobby();
            }
        }

        public void CreateRoom()
        {
            RoomRequestBody createRoom = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = client.BodyStructToBytes(createRoom);
            client.Send(MessageType.Room_Create, body);
            Header h = (Header)client.GetHeader();

            if (h.state == MessageState.SUCCESS)
            {
                byte[] roomno = client.Recieve(h.length);
                int no = BitConverter.ToInt32(roomno,0);
                Console.WriteLine(no);
                EnterRoom(no);
            }
            else
            {
                Console.WriteLine("[!]Fail to Create Room");
            }
        }

        public void EnterRoom(int roomno)
        {
            RoomRequestBody EnterRoom = new RoomRequestBody(user.id.ToCharArray(), 0);
            byte[] body = client.BodyStructToBytes(EnterRoom);
            client.Send(MessageType.Room_Join, body);

            Header h = (Header)client.GetHeader();

            if (h.state == MessageState.SUCCESS)
            {
                Console.WriteLine("["+roomno+"] 번 방에 입장하셨습니다.");
            }
            else
            {
                Console.WriteLine("[!]Fail to get Room List");
            }

        }
        public bool exit()
        {
            Console.WriteLine("Do you want to exit?(y/n)");
            return true;
        }
    }
}