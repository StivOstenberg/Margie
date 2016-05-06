using System;
using System.Collections.Generic;
using MargieBot.Models;

namespace MargieBot.Responders
{
    public class SimpleResponder : IResponder
    {
        /// <summary>
        /// This is a comment
        /// </summary>
        public Func<ResponseContext, bool> CanRespondFunction { get; set; } 
        /// <summary>
        /// I, too, am a comment
        /// </summary>
        public List<Func<ResponseContext, BotMessage>> GetResponseFunctions { get; set; }

        /// <summary>
        /// Yet anotehr comment.
        /// </summary>
        public SimpleResponder()
        {
            GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>();
        }

        /// <summary>
        /// What to do what to do?
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanRespond(ResponseContext context)
        {
            return CanRespondFunction(context);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            if (GetResponseFunctions.Count == 0) {
                throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
            }

            return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
        }
    }
}