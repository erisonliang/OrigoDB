﻿using System;
using FakeItEasy;
using NUnit.Framework;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class CommandTimestampTests
    {
        [Serializable]
        class TestModel : Model
        {
            public DateTime Sometime;

            public void SetTime(DateTime time)
            {
                Sometime = time;
            }
        }

        [Serializable]
        class SetTimeCommand : Command<TestModel>
        {

            public override void Execute(TestModel model)
            {
                var ts = Execution.Current.Now;
                model.SetTime(ts);
            }
        }

        [Test]
        public void Timestamp_is_copied_to_journal_entry()
        {
            JournalEntry entry = null;

            var fake = A.Fake<IJournalWriter>();
            
            A.CallTo(() => fake.Write(A<JournalEntry>._))
                .Invokes((JournalEntry je) =>
                {
                    entry = je;
                });

            var before = DateTime.Now;
            Execution.Begin();
            var command = new SetTimeCommand();
            var target = new JournalAppender(0, fake);
            var after = DateTime.Now;
            target.Append(command);

            Assert.IsNotNull(entry);
            Assert.IsTrue(before <= entry.Created && entry.Created <= after);
        }

        [Test]
        public void Timestamp_preserved_on_restore()
        {
            var command = new SetTimeCommand();
            var config = new EngineConfiguration().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);

            DateTime timeStamp = default(DateTime);
            engine.CommandExecuted += (sender, args) => { timeStamp = args.StartTime; };
            engine.Execute(command);
            engine.Close();

            engine = Engine.Load<TestModel>(config);
            Assert.AreEqual(timeStamp, ((TestModel) engine.GetModel()).Sometime);
        }
    }
}