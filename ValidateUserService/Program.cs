using System;
using Opc.UaFx.Client;
using Opc.UaFx;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ValidateUserService
{
    public class MachineUser
    {
        public OpcValue i_OpcValUserLogginId { get; set; }
        
        public class CheckResult
        {
            public int FoundPersonalCardId { get; set; }
            public int FoundPermitionLevel { get; set; }
        }

        public CheckResult CheckAccessLevel(OpcValue OpcUserId)
        {
            int FoundUserId = 0;
            int FoundAccessLevel = 0;
            bool UserFound = false;
            const int NotFoundErrorCode = 9999;
            string connetionString;

            //start checking user in db - when value from opc is not 0 or not null
            if ( !Equals(OpcUserId.ToString(), "0") && OpcUserId.Value != null && OpcUserId.Status.IsGood )
            {
                //Retrieve the scalar value of the OpcValue instance 
                UInt16 TryUserId = (UInt16)OpcUserId.Value;

                SqlConnection connection;
                connetionString = @"Data Source=localhost;Database=CaSecurity;Trusted_Connection=True;MultipleActiveResultSets=true";
                connection = new SqlConnection(connetionString);
                connection.Open();

                try
                {
                    string query = "SELECT PersonalCardId FROM Users WHERE PersonalCardId='" + TryUserId + "'";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader readerGetUserId = cmd.ExecuteReader();

                    if (readerGetUserId.Read())
                    {
                        UserFound = true;
                        FoundUserId = readerGetUserId.GetInt32(0);

                    }
                    else
                    {
                        UserFound = false;
                        FoundUserId = NotFoundErrorCode;
                    }
                    readerGetUserId.Close();

                    string queryGetPermitionLevel = "SELECT PermitionLevel FROM Users WHERE PersonalCardId='" + TryUserId + "'";
                    SqlCommand cmdGetPermitionLevel = new SqlCommand(queryGetPermitionLevel, connection);
                    SqlDataReader readerGetPermitionLevel = cmdGetPermitionLevel.ExecuteReader();

                    if ( UserFound && readerGetPermitionLevel.Read() )
                        { 
                            FoundAccessLevel = readerGetPermitionLevel.GetInt32(0);
                            
                        }
                    readerGetPermitionLevel.Close();
                }
                finally
                {
                    //db.complete();
                    connection.Close();
                }

                /////////////////////////////////////////////////
            }

            //}

            //// Tag5
            //OpcSubscription subscription = client.SubscribeDataChange("ns=2;s=Channel2.Device1.Tag5", HandleDataChanged);
            //subscription.PublishingInterval = 1000;
            //subscription.ApplyChanges();

            // 一Writing Tags

            //OpcWriteNode[] wCommands = new OpcWriteNode[] {
            //new OpcWriteNode("ns=2;s=GDi-L1_Injector-Assembly.SelenoidWeld.o_iTargetPartsPerShift", UserIdAck),    //  boolean
            //new OpcWriteNode("ns=2;s=GDi-L1_Injector-Assembly.SelenoidWeld.i_iTargetScore", 0), 
            //new OpcWriteNode("ns=2;s=Channel2.Device1.Tag2", "Test"),   //  sting
            //new OpcWriteNode("ns=2;s=Channel2.Device1.Tag3", 8.7),      //  float
            //new OpcWriteNode("ns=2;s=Channel2.Device1.Tag3", (ushort)88)//  word

            //};
            //OpcStatusCollection results = client.WriteNodes(wCommands);

            var checkResult = new CheckResult
            {
                FoundPersonalCardId = FoundUserId,
                FoundPermitionLevel = FoundAccessLevel
            };

            return checkResult;
    }

    }
    class Program
    {
        private static void HandleDataChanged(object sender,OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            Console.WriteLine("DataChange: {0} = {1}", item.NodeId,e.Item.Value);
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////

        static void Main(string[] args)
        {
            //set opc ua source
            var client = new OpcClient("opc.tcp://127.0.0.1:49328");
            
            //client.Security.AutoAcceptUntrustedCertificates = true;
            client.Security.UserIdentity = new OpcClientIdentity("Admin", "zaq12wsx");
            client.Security.EndpointPolicy = new OpcSecurityPolicy( OpcSecurityMode.SignAndEncrypt, OpcSecurityAlgorithm.Basic256);
            client.Connect();

            //Initialize call to User instance for each Machine
            MachineUser SolenoidWeldUser = new MachineUser();

            

            //infinite loop
            while (true)
            {
                //Initialize call to User instance for each Machine
            
                //SolenoidWeldUser.CheckResult.FoundPermitionLevel = SolenoidWeldUser.CheckAccessLevel(SolenoidWeldUser.i_OpcValUserLogginId);
                SolenoidWeldUser.i_OpcValUserLogginId = client.ReadNode("ns=2;s=GDi-L1_Injector-Assembly.SelenoidWeld.i_iSpare1");
                
                var CheckResult = SolenoidWeldUser.CheckAccessLevel(SolenoidWeldUser.i_OpcValUserLogginId);


                Console.WriteLine(CheckResult.FoundPermitionLevel);
                Console.WriteLine(CheckResult.FoundPersonalCardId);
            }

            //Console.ReadLine();

            client.Disconnect();
            //connection.Close();
        }
    }
}
