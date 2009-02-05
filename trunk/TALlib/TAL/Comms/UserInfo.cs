using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TALlib.TAL.Comms
{
    public class UserInfo
    {
        /// <summary>
        /// The UserInfo constructor for users who want to trade in the first 
        /// BANK,BRANCH,CUSTOMER,DEPOSIT account listed in their user_perms.xml file. 
        /// This works well for users who only have one or two accounts mapped. 
        /// </summary>
        public UserInfo()
        {
            szFirstRoute = "";
            PopulateLoginIdFromXML(GetTALXmlFile());
            PopulateBBCDFromXML(GetTALXmlFile());
            omsClientType = 1;
        }

        /// <summary>
        /// The UserInfo constructor for users who want to trade in the first 
        /// BANK,BRANCH,CUSTOMER,DEPOSIT account listed in their user_perms.xml file. 
        /// This works well for users who only have one or two accounts mapped. 
        /// </summary>
        public UserInfo(bool bDontRequirePrimary)
        {
            szFirstRoute = "";
            PopulateLoginIdFromXML(GetTALXmlFile(), bDontRequirePrimary);
            PopulateBBCDFromXML(GetTALXmlFile(), bDontRequirePrimary);
            omsClientType = 1;
        }

        /// <summary>
        /// The UserInfo constructor for users who want to specify the 
        /// BANK,BRANCH,CUSTOMER,DEPOSIT account in which they would like 
        /// to trade, since their user has multiple accounts mapped. 
        /// </summary>
        /// <param name="szBank"></param>
        /// <param name="szBranch"></param>
        /// <param name="szCust"></param>
        /// <param name="szDep"></param>
        public UserInfo(string szBank, string szBranch, string szCust, string szDep)
        {
            szFirstRoute = "";
            PopulateLoginIdFromXML(GetTALXmlFile());
            bank = szBank;
            branch = szBranch;
            customer = szCust;
            deposit = szDep;
            omsClientType = 1;
        }

        
        /// <summary>
        /// The UserInfo constructor are for users who want to manually specify all parms. 
        /// </summary>
        /// <param name="szBank"></param>
        /// <param name="szBranch"></param>
        /// <param name="szCust"></param>
        /// <param name="szDep"></param>
        /// <param name="szLogin"></param>
        public UserInfo(string szBank, string szBranch, string szCust, string szDep, string szLogin)
        {
            szFirstRoute = "";
            bank = szBank;
            branch = szBranch;
            customer = szCust;
            deposit = szDep;
            loginid = szLogin;
            omsClientType = 1;
        }


        /// <summary>
        /// I am currently not aware of any situation where you would want OMS_CLIENT_TYPE 
        /// to be other than 1, but I know this is theoretically possible.
        /// </summary>
        /// <param name="szBank"></param>
        /// <param name="szBranch"></param>
        /// <param name="szCust"></param>
        /// <param name="szDep"></param>
        /// <param name="szLogin"></param>
        /// <param name="iOMSClientType"></param>
        public UserInfo(string szBank, string szBranch, string szCust, string szDep, string szLogin,
            int iOMSClientType)
        {
            szFirstRoute = "";
            bank = szBank;
            branch = szBranch;
            customer = szCust;
            deposit = szDep;
            loginid = szLogin;
            omsClientType = iOMSClientType;
        }



        public string username;
        public string domain;
        public string bank;
        public string branch;
        public string customer;
        public string deposit;
        public string loginId;
        public int omsClientType;	//default is 1


        //Find location of configuration files (XML) that will tell us what TRADER_ID we 
        //are logged in as,
        //and what BANK,BRANCH,CUSTOMER,DEPOSIT accounts we are linked to. 
        public static string GetTalDir()
        {
            string talDir = Environment.GetEnvironmentVariable("TALDIR");
            if (!(talDir.EndsWith("\\")))
            {
                talDir += "\\";
            }
            return talDir;
        }


        /// <summary>
        /// This will return the first route that is permissioned in the first BBCD 
        /// IF AND ONLY IF the user has called one of the constructor overloads 
        /// that don't specify the BBCD. The thinking is, if you know the BBCD, you 
        /// probably know the route you want as well. 
        /// </summary>
        /// <returns></returns>
        public string GetFirstRoute()
        {
            return (szFirstRoute);
        }





        //Get the first locale mentioned in the locale.xml file. 
        //This works well for users who are only permed for one or two locales (i.e. most users). 
        //However, users who are permed for many locales will want to specify the locale
        //string manually, since the first locale might not be the best.  Therefore, their user
        //code will probably hardcode the locale string or read it from somewhere else. 
        public static string GetFirstLocaleFromLocaleXML()
        {
            string szLocale;
            XmlTextReader localeReader = new XmlTextReader(GetTalDir() + "data\\locale.xml");
            while (localeReader.Read())
            {
                if (localeReader.Name.Equals("Locale"))
                {
                    szLocale = localeReader.GetAttribute("Name");
                    localeReader.Close();
                    return szLocale;
                }
            }
            throw new Exception("Locale XML did not contain any locales!");
        }

        private void PopulateLoginIdFromXML(XmlTextReader userPermsReader)
        {
            bool bUserDomain = false;
            while (userPermsReader.Read())
            {
                switch (userPermsReader.Name)
                {
                    case "s":
                        while (!(userPermsReader.GetAttribute("ServerType").Equals("TS3") && userPermsReader.GetAttribute("Primary").Equals("Y")))
                        {
                            userPermsReader.Skip();
                        }
                        if (userPermsReader.EOF)
                        {
                            throw new Exception("user_perms.xml does not contain a primary TS3 account!");
                        }
                        break;

                    case "v":
                        username = userPermsReader.GetAttribute("Name");
                        bUserDomain = (!(username == null));
                        if (!bUserDomain) break;
                        domain = userPermsReader.GetAttribute("Domain");
                        loginid = username + "@" + domain;
                        break;
                }
                if (bUserDomain)
                {
                    userPermsReader.Close();
                }
            }
            if (!bUserDomain)
            {
                throw new Exception("user_perms.xml did not contain the USERNAME@DOMAIN!");
            }

        }

        private void PopulateLoginIdFromXML(XmlTextReader userPermsReader, bool bDontRequirePrimary)
        {
            bool bUserDomain = false;
            while (userPermsReader.Read())
            {
                switch (userPermsReader.Name)
                {
                    case "s":
                        while (!(userPermsReader.GetAttribute("ServerType").Equals("TS3") && bDontRequirePrimary || userPermsReader.GetAttribute("Primary").Equals("Y")))
                        {
                            userPermsReader.Skip();
                        }
                        if (userPermsReader.EOF)
                        {
                            throw new Exception("user_perms.xml does not contain a primary TS3 account!");
                        }
                        break;

                    case "v":
                        username = userPermsReader.GetAttribute("Name");
                        bUserDomain = (!(username == null));
                        if (!bUserDomain) break;
                        domain = userPermsReader.GetAttribute("Domain");
                        loginid = username + "@" + domain;
                        break;
                }
                if (bUserDomain)
                {
                    userPermsReader.Close();
                }
            }
            if (!bUserDomain)
            {
                throw new Exception("user_perms.xml did not contain the USERNAME@DOMAIN!");
            }

        }

        private void PopulateBBCDFromXML(XmlTextReader userPermsReader)
        {
            bool bBank = false;
            bool bBranch = false;
            bool bCust = false;
            bool bDep = false;
            bool bRoute = false;
            bool bDone = false;

            string szPermType;
            while (userPermsReader.Read())
            {
                switch (userPermsReader.Name)
                {
                    case "s":
                        while (!(userPermsReader.GetAttribute("ServerType").Equals("TS3") && userPermsReader.GetAttribute("Primary").Equals("Y")))
                        {
                            userPermsReader.Skip();
                        }
                        if (userPermsReader.EOF)
                        {
                            throw new Exception("user_perms.xml does not contain a primary TS3 account!");
                        }
                        break;

                    case "bk":
                        bank = userPermsReader.GetAttribute("Bank");
                        bBank = (!(bank == null));
                        break;
                    case "br":
                        branch = userPermsReader.GetAttribute("Branch");
                        bBranch = (!(branch == null));
                        break;
                    case "c":
                        customer = userPermsReader.GetAttribute("Customer");
                        bCust = (!(customer == null));
                        break;
                    case "d":
                        deposit = userPermsReader.GetAttribute("Account");
                        bDep = (!(deposit == null));
                        break;
                    case "p":
                        szPermType = userPermsReader.GetAttribute("PermType");
                        if (szPermType != null)
                        {
                            if (szPermType.Equals("Routing"))
                            {
                                szFirstRoute = userPermsReader.GetAttribute("PermDesc");
                                bRoute = (!(szFirstRoute == null));
                            }
                        }
                        break;
                    default:
                        break;
                }
                bDone = (bBank && bBranch && bCust && bDep && bRoute);
                if (bDone)
                {
                    userPermsReader.Close();
                }
            }
            if (!bDone)
            {
                throw new Exception("user_perms.xml did not contain properly formatted BBCD information");
            }
        }

        private void PopulateBBCDFromXML(XmlTextReader userPermsReader, bool bDontRequirePrimary)
        {
            bool bBank = false;
            bool bBranch = false;
            bool bCust = false;
            bool bDep = false;
            bool bRoute = false;
            bool bDone = false;

            string szPermType;
            while (userPermsReader.Read())
            {
                switch (userPermsReader.Name)
                {
                    case "s":
                        while (!(userPermsReader.GetAttribute("ServerType").Equals("TS3") && bDontRequirePrimary || userPermsReader.GetAttribute("Primary").Equals("Y")))
                        {
                            userPermsReader.Skip();
                        }
                        if (userPermsReader.EOF)
                        {
                            throw new Exception("user_perms.xml does not contain a primary TS3 account!");
                        }
                        break;

                    case "bk":
                        bank = userPermsReader.GetAttribute("Bank");
                        bBank = (!(bank == null));
                        break;
                    case "br":
                        branch = userPermsReader.GetAttribute("Branch");
                        bBranch = (!(branch == null));
                        break;
                    case "c":
                        customer = userPermsReader.GetAttribute("Customer");
                        bCust = (!(customer == null));
                        break;
                    case "d":
                        deposit = userPermsReader.GetAttribute("Account");
                        bDep = (!(deposit == null));
                        break;
                    case "p":
                        szPermType = userPermsReader.GetAttribute("PermType");
                        if (szPermType != null)
                        {
                            if (szPermType.Equals("Routing"))
                            {
                                szFirstRoute = userPermsReader.GetAttribute("PermDesc");
                                bRoute = (!(szFirstRoute == null));
                            }
                        }
                        break;
                    default:
                        break;
                }
                bDone = (bBank && bBranch && bCust && bDep && bRoute);
                if (bDone)
                {
                    userPermsReader.Close();
                }
            }
            if (!bDone)
            {
                throw new Exception("user_perms.xml did not contain properly formatted BBCD information");
            }
        }

        private XmlTextReader GetTALXmlFile()
        {
            return (new XmlTextReader(GetTalDir() + "data\\user_perms.xml"));
        }



        public override int GetHashCode()
        {
            return (loginid.GetHashCode() ^
                bank.GetHashCode() ^
                branch.GetHashCode() ^
                customer.GetHashCode() ^
                deposit.GetHashCode());
        }


        public override bool Equals(object obj)
        {
            if (!(obj is UserInfo)) return false;

            UserInfo u = obj as UserInfo;
            return (u.loginid.Equals(this.loginid) &&
                u.bank.Equals(this.bank) &&
                u.branch.Equals(this.branch) &&
                u.customer.Equals(this.customer) &&
                u.deposit.Equals(this.deposit));
        }



    }
}
