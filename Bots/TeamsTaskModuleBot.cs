// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.BotBuilderSamples.Models;
using System;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsTaskModuleBot : TeamsActivityHandler
    {
        private readonly string _baseUrl;
        private static readonly int SongListBatchLen = 6;
        private static UISettings [] AllAlbumUIList;
        private static string [] AllAlbumList;

        public TeamsTaskModuleBot(IConfiguration config)
        {
            _baseUrl = config["BaseUrl"];
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //var reply = MessageFactory.Attachment(new[] { GetTaskModuleHeroCardOptions(), GetTaskModuleAdaptiveCardOptions() });

            var reply = MessageFactory.Attachment(new[] { GetTaskModuleHeroCardOptions()});

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;

            var taskInfo = new TaskModuleTaskInfo();

            int n1 = int.Parse(value);
            int n2 = AllAlbumList.Count();

            n1 = n1 < 0 ? 0 : n1;
            n1 = n1 >= n2 ? n2 - 1 : n1;

            taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/Music/MusicPlayerFrame/" + n1;

            SetTaskInfo(taskInfo, AllAlbumUIList[n1]);

            /*
            switch (value)
            {
                case TaskModuleIds.YouTube:
                    //taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/" + TaskModuleIds.YouTube;
                    taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/Music/MusicPlayer/1";

                    SetTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
                    break;
                case TaskModuleIds.CustomForm:
                    //taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/" + TaskModuleIds.CustomForm;
                    taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/Music/MusicPlayerFrame/1";

                    SetTaskInfo(taskInfo, TaskModuleUIConstants.CustomForm);
                    break;
                case TaskModuleIds.AdaptiveCard:
                    taskInfo.Card = CreateAdaptiveCardAttachment();
                    SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
                    break;
                default:
                    break;
            }
            //*/

            return Task.FromResult(taskInfo.ToTaskModuleResponse());
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + JsonConvert.SerializeObject(taskModuleRequest));
            await turnContext.SendActivityAsync(reply, cancellationToken);

            return TaskModuleResponseFactory.CreateResponse("Thanks!");
        }

        private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }

        private static Attachment GetTaskModuleHeroCardOptions()
        {
            // Create a Hero Card with TaskModuleActions for each Task Module

            string dirPath = "wwwroot/MusicLibs";
            List<string> dirs = new List<string>(Directory.GetDirectories(dirPath, "*", System.IO.SearchOption.AllDirectories));

            // save a copy
            AllAlbumList = dirs.ToArray();

            System.Random rd = new Random();
            int n1 = dirs.Count();

            var dirsidx = new List<int>();
            for (int i = 0; i < n1; i++)
            {
                dirsidx.Add(i);
            }

            // shuttle album list
            for (int i = 0; i < n1; i++)
            {
                int n2 = rd.Next(n1);

                string t1 = dirs[n2];
                int n3 = dirsidx[n2];
                dirs[n2] = dirs[i];
                dirsidx[n2] = dirsidx[i];
                dirs[i] = t1;
                dirsidx[i] = n3;
            }

            AllAlbumUIList = new UISettings[n1];

            for (int i = 0; i < n1; i++)
            {
                string t1 = Path.GetFileName(AllAlbumList[i]);
                AllAlbumUIList[i] = new UISettings(1200, 1000, "Album: " + t1, "" + i, t1);
            }

            var SongUIList = new UISettings[SongListBatchLen];

            for (int i = 0; i < SongListBatchLen; i++)
            {
                string t1 = Path.GetFileName(dirs[i]);
                SongUIList[i] = new UISettings(1200, 1000, "Album: " + t1, "" + dirsidx[i], t1);
            }

            return new HeroCard()
            {
                Title = "Please select your song album: ",
                //Buttons = new[] { TaskModuleUIConstants.AdaptiveCard, TaskModuleUIConstants.CustomForm, TaskModuleUIConstants.YouTube }
                //            .Select(cardType => new TaskModuleAction(cardType.ButtonTitle, new CardTaskFetchValue<string>() { Data = cardType.Id }))
                //            .ToList<CardAction>(),

                Buttons = SongUIList.Select(cardType => new TaskModuleAction(cardType.ButtonTitle, new CardTaskFetchValue<string>() { Data = cardType.Id }))
                            .ToList<CardAction>(),
            }.ToAttachment();
        }

        private static Attachment GetTaskModuleAdaptiveCardOptions()
        {
            // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock(){ Text="Task Module Invocation from Adaptive Card", Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
                    },
                Actions = new[] { TaskModuleUIConstants.AdaptiveCard, TaskModuleUIConstants.CustomForm, TaskModuleUIConstants.YouTube }
                            .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = cardType.Id } })
                            .ToList<AdaptiveAction>(),
            };

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        private static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
