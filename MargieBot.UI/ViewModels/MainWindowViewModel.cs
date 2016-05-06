﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Windows.Input;
using Bazam.Wpf.UIHelpers;
using Bazam.Wpf.ViewModels;
using MargieBot.Models;
using MargieBot.ExampleResponders.Models;
using MargieBot.ExampleResponders.Responders;
using MargieBot.Responders;
using System.Configuration;

namespace MargieBot.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase<MainWindowViewModel>
    {
        private Bot _Margie;



        private string _AuthKeySlack = string.Empty;
        public string AuthKeySlack
        {
            get { return _AuthKeySlack; }
            set { ChangeProperty(vm => vm.AuthKeySlack, value); }
        }

        private string _BotUserID = string.Empty;
        public string BotUserID
        {
            get { return _BotUserID; }
            set { ChangeProperty(vm => vm.BotUserID, value); }
        }

        private string _BotUserName = string.Empty;
        public string BotUserName
        {
            get { return _BotUserName; }
            set { ChangeProperty(vm => vm.BotUserName, value); }
        }

        private IReadOnlyList<SlackChatHub> _ConnectedHubs;
        public IReadOnlyList<SlackChatHub> ConnectedHubs
        {
            get { return _ConnectedHubs; }
            set { ChangeProperty(vm => vm.ConnectedHubs, value); }
        }

        private DateTime? _ConnectedSince = null;
        public DateTime? ConnectedSince
        {
            get { return _ConnectedSince; }
            set { ChangeProperty(vm => vm.ConnectedSince, value); }
        }

        private bool _ConnectionStatus = false;
        public bool ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set { ChangeProperty(vm => vm.ConnectionStatus, value); }
        }

        private List<string> _Messages = new List<string>();
        public IEnumerable<string> Messages
        {
            get { return _Messages; }
        }

        private string _MessageToSend = string.Empty;
        public string MessageToSend
        {
            get { return _MessageToSend; }
            set { ChangeProperty(vm => vm.MessageToSend, value); }
        }

        private SlackChatHub _SelectedChatHub;
        public SlackChatHub SelectedChatHub
        {
            get { return _SelectedChatHub; }
            set { ChangeProperty(vm => vm.SelectedChatHub, value); }
        }

        private string _TeamName = string.Empty;
        public string TeamName
        {
            get { return _TeamName; }
            set { ChangeProperty(vm => vm.TeamName, value); }
        }

        public ICommand ConnectCommand
        {



            get { 
                return new RelayCommand(async () => {
                    if (_Margie != null && ConnectionStatus) {
                        SelectedChatHub = null;
                        ConnectedHubs = null;
                        _Margie.Disconnect();
                    }
                    else {
                        // let's margie
                        _Margie = new Bot();
                        _Margie.Aliases = GetAliases();
                        foreach(KeyValuePair<string, object> value in GetStaticResponseContextData()) {
                            _Margie.ResponseContext.Add(value.Key, value.Value);
                        }
                        
                        // RESPONDER WIREUP
                        _Margie.Responders.AddRange(GetResponders());

                        _Margie.ConnectionStatusChanged += (bool isConnected) => {
                            ConnectionStatus = isConnected;

                            if (isConnected) {
                                // now that we're connected, build list of connected hubs for great glory
                                List<SlackChatHub> hubs = new List<SlackChatHub>();
                                hubs.AddRange(_Margie.ConnectedChannels);
                                hubs.AddRange(_Margie.ConnectedGroups);
                                hubs.AddRange(_Margie.ConnectedDMs);
                                ConnectedHubs = hubs;

                                if (ConnectedHubs.Count > 0) {
                                    SelectedChatHub = ConnectedHubs[0];
                                }

                                // also set other cool properties
                                BotUserID = _Margie.UserID;
                                BotUserName = _Margie.UserName;
                                ConnectedSince = _Margie.ConnectedSince;
                                TeamName = _Margie.TeamName;
                            }
                            else {
                                ConnectedHubs = null;
                                BotUserID = null;
                                BotUserName = null;
                                ConnectedSince = null;
                                TeamName = null;
                            }
                        };
                        _Margie.MessageReceived += (string message) => {
                            int messageCount = _Messages.Count - 500;
                            for (int i = 0; i < messageCount; i++) {
                                _Messages.RemoveAt(0);
                            }

                            _Messages.Add(message);
                            RaisePropertyChanged("Messages");
                        };

                        await _Margie.Connect(AuthKeySlack); 
                    }
                }); 
            }
        }

        public ICommand TalkCommand
        {
            get
            {
                return new RelayCommand(async () => {
                    await _Margie.Say(new BotMessage() { Text = MessageToSend, ChatHub = SelectedChatHub });
                    MessageToSend = string.Empty;
                });
            }
        }

        /// <summary>
        /// Replace the contents of the list returned from this method with any aliases you might want your bot to respond to. If you
        /// don't want your bot to respond to anything other than its actual name, just return an empty list here.
        /// </summary>
        /// <returns>A list of aliases that will cause the BotWasMentioned property of the ResponseContext to be true</returns>
        private IReadOnlyList<string> GetAliases()
        {
            return new List<string>() { "trycorder","try" };
        }

        /// <summary>
        /// If you want to use this application to run your bot, here's where you start. Just scrap as many of the responders
        /// described in this method as you want and start fresh. Define your own responders using the methods describe
        /// at https://github.com/jammerware/margiebot/wiki/Configuring-responses and return them in an IList<IResponder>. 
        /// You create them in this project, in a separate one, or even in the ExampleResponders project if you want.
        /// 
        /// Boom! You have your own bot.
        /// </summary>
        /// <returns>A list of the responders this bot should respond with.</returns>
        private IList<IResponder> GetResponders()
        {
            // Some of these are more complicated than they need to be for the sake of example
            List<IResponder> responders = new List<IResponder>();

            // examples of semi-complex or "messier" responders (created in separate classes)
            //responders.Add(new ScoreResponder());
            //responders.Add(new ScoreboardRequestResponder());
            responders.Add(new WhatsNewResponder());
            //responders.Add(new WikipediaResponder());
            responders.Add(new EC2Responder());


            // examples of simple-ish "inline" responders
            // this one hits on Slackbot when he talks 1/8 times or so
            //_Margie.Responders.Add(_Margie.CreateResponder(
             //   (ResponseContext context) => { return (context.Message.User.IsSlackbot && new Random().Next(8) <= 1); },
             //   (ResponseContext context) => { return context.Get<Phrasebook>().GetSlackbotSalutation(); }));

            // easiest one of all - this one responds if someone thanks Margie
            responders.Add(_Margie.CreateResponder(
                (ResponseContext context) => { return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase); },
                (ResponseContext context) => { return context.Get<Phrasebook>().GetYoureWelcome(); }
            ));

            // example of Supa Fly Mega EZ Syntactic Sugary Responder (not their actual name)
            _Margie
                .RespondsTo("get on that")
                .With("Sure, hun!")
                .With("I'll see what I can do, sugar.")
                .With("I'll try. No promises, though!")
                .IfBotIsMentioned();

            // you can do these with regexes too
            _Margie
                .RespondsTo("what (can|do) you do", true)
                .With(@"Still trying to figger that out!")
                .IfBotIsMentioned();

            // this last one just responds if someone says "hi" or whatever to Margie, but only if no other responder has responded
            responders.Add(_Margie.CreateResponder(
                (ResponseContext context) => {
                    return
                        context.Message.MentionsBot &&
                        !context.BotHasResponded &&
                        Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello|what's up|what's happening)\b", RegexOptions.IgnoreCase) &&
                        context.Message.User.ID != context.BotUserID &&
                        !context.Message.User.IsSlackbot;
                },
                (ResponseContext context) => {
                    return context.Get<Phrasebook>().GetQuery();
                }
            ));

            return responders;
        }

        /// <summary>
        /// If you want to share any data across all your responders, you can use the StaticResponseContextData property of the bot to do it. I elected
        /// to have most of my responders use a "Phrasebook" object to ensure a consistent tone across the bot's responses, so I stuff the Phrasebook
        /// into the context for use.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> GetStaticResponseContextData()
        {
            return new Dictionary<string, object>() { 
                { "Phrasebook", new Phrasebook() }
            };
        }
    }
}