using EFSeminar.Models;
using EFSeminar.Services;

namespace SeverTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            using (var ctx = new ChatContext())
            {
                ctx.Messages.RemoveRange(ctx.Messages);
                ctx.Users.RemoveRange(ctx.Users);
                ctx.SaveChanges();
            }
        }
        [TearDown]
        public void Teardown()
        {
            using (var ctx = new ChatContext())
            {
                ctx.Messages.RemoveRange(ctx.Messages);
                ctx.Users.RemoveRange(ctx.Users);
                ctx.SaveChanges();
            }
        }
        [Test]
        public async Task Test()
        {
            var mock = new MockMessageSource();
            var srv = new Server(mock);
            mock.AddServer(srv);
            await srv.Work();
            using (var ctx = new ChatContext())
            {
                Assert.IsTrue(ctx.Users.Count() == 2, "Пользователи не найдены.");

                var user1 = ctx.Users.FirstOrDefault(x => x.FullName == "Vasya");
                var user2 = ctx.Users.FirstOrDefault(x => x.FullName == "Elena");

                Assert.IsNotNull(user1, "Пользователи не созданы");
                Assert.IsNotNull(user2, "Пользователи не созданы");

                Assert.IsTrue(user1.MessagesFrom.Count == 1);
                Assert.IsTrue(user2.MessagesFrom.Count == 1);

                Assert.IsTrue(user1.MessagesTo.Count == 1);
                Assert.IsTrue(user2.MessagesTo.Count == 1);

                var msg1 = ctx.Messages.FirstOrDefault(x => x.UserFrom == user1 && x.UserTo == user2);
                var msg2 = ctx.Messages.FirstOrDefault(x => x.UserFrom == user2 && x.UserTo == user1);

                Assert.AreEqual("From Elena", msg2.Text);
                Assert.AreEqual("From Vasya", msg1.Text);
            }
        }
    }
}
