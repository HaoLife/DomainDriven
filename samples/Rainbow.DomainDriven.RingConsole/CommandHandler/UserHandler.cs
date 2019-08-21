using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.RingConsole.Command;
using System.Linq;
using Rainbow.DomainDriven.RingConsole.Domain;

namespace Rainbow.DomainDriven.RingConsole.CommandExecutor
{
    public class UserHandler :
        ICommandHandler<CreateUserCommand>,
        ICommandHandler<ModifyUserNameCommand>,
        ICommandHandler<ModifyUserSexCommand>,
        ICommandHandler<RemoveUserCommand>
    {
        public UserHandler(User uer)
        {

        }


        // private readonly UserService _userService;
        // public UserHandler(UserService userService)
        // {
        //     this._userService = userService;
        // }

        public void Handle(ICommandContext context, RemoveUserCommand cmd)
        {
            //context.Get<User>(cmd.UserId).Dispose();
        }

        public void Handle(ICommandContext context, ModifyUserSexCommand cmd)
        {
            context.Get<User>(cmd.UserId).ModifySex(cmd.Sex);
        }

        public void Handle(ICommandContext context, ModifyUserNameCommand cmd)
        {
            //var repo = context.Repo<UserRepository>();
            //var isExists = repo.Exists(cmd.Name);
            //if (isExists)
            //    throw new DomainException(1, "已存在该名称的用户，请更换一个新的名称");

            context.Get<User>(cmd.UserId).ModifyName(cmd.Name);
        }

        public void Handle(ICommandContext context, CreateUserCommand cmd)
        {
            var userRoot = context.Get<User>(cmd.UserId);

            //var r2 = context.Get<User>(cmd.UserId2);
            //var r3 = context.Get<User>(cmd.UserId3);

            //throw new DomainDriven.Domain.DomainException(100,"故意出错");
            var user = new User(cmd.UserId, cmd.Name, cmd.Sex);
            //_userService.RegisterUser(user);
            user.ModifyName(cmd.Name);
            context.Add(user);
        }
    }



}
