﻿using System;
using System.Reflection;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;


using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Collections.Generic;

namespace MargieBot.Responders
{
    public class EC2Responder : IResponder
    {
        //Need to initialize connection to Trycorder..
        //Need trycorder based off the service references

        TrycorderWebReference.ScannerClass Trycorder = new TrycorderWebReference.ScannerClass();
        AWSFunctions.ScanAWS stivfunc = new AWSFunctions.ScanAWS();

        public bool CanRespond(ResponseContext context)
        {
            bool testcondition = (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM);
            return testcondition;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            BotMessage toreturn = new BotMessage();
            string whoasksid = context.Message.User.ID;
            string alias = context.UserNameCache[whoasksid];

            //Need to initialize connection to Trycorder..
            //ScannerEngine.ScannerClass Trycorder = new ScannerEngine.ScannerClass();
            // AWSFunctions.ScanAWS stivfunc = new AWSFunctions.ScanAWS();



            string phrase = context.Message.Text;
            var args = new List<string>(phrase.Split(' '));
            if (args.Count<=1||args[1].ToLower().Contains("help") || args[1].ToLower().Contains("usage") || args[1].ToLower().Contains("?"))
            {
                toreturn.Text = usage();
                return toreturn;
            }
            switch (args[1].ToLower())
            {
                case "ec2":
                    toreturn.Text= ec2processor(args);
                    break;
                case "rds":
                    break;
                case "scan":
                    try
                    {
                        string data = Trycorder.ScanAll();
                        toreturn.Text = data;
                    }
                    catch(Exception ex)
                    {
                        toreturn.Text = "Trycorder Scan failed!\n" + ex;
                    }
                    break;
                case "status":
                    try
                    {

                        toreturn.Text = Trycorder.GetDetailedStatus();
                    }
                    catch (Exception ex)
                    {
                        toreturn.Text = "Trycorder connection failed!\n" + ex;
                    }
                    break;
                case "profiles":
                    try
                    {
                        var Proflist = Trycorder.GetProfiles();
                        List<string> profs = new List<String>();
                        foreach(var rabbit in Proflist)
                        {
                            profs.Add(rabbit.Key);
                        }
                        toreturn.Text = stivfunc.List2String(profs);
                    }
                    catch(Exception ex)
                    {
                        toreturn.Text = "Trycorder connection failed!\n" + ex;
                    }
                    break;
                default:
                    break;
            }

            if(context.Message.Text.ToLower().EndsWith("private"))
            {
                toreturn.ChatHub = context.Message.ChatHub;
                toreturn.ChatHub.Type = SlackChatHubType.DM;
                toreturn.ChatHub.ID = whoasksid;
                toreturn.ChatHub.Name = "@" + whoasksid;   
            }

            return toreturn;

        }

        public string usage()
        {
            string ToReturn = "";
            ToReturn += " Trycorder commands: \n";
            ToReturn += "Scan\n";
            ToReturn += "Profiles\n";
            ToReturn += "Status\n";

            return ToReturn;
        }
        /// <summary>
        /// If trycorder is passed an EC2 argument,  process here.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string ec2processor(List<string> args)
        {
            string ToReturn = "";
            string filterstring = args[2];

            var what = Trycorder.FilterScannerDataTable("ec2", filterstring, true, true);

            var number = what.Rows.Count;

            foreach(var arow in what.Rows)
            {
                string account = "";

            }



            return ToReturn;
        }

    }
}