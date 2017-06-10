using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.ConsoleApp.Command;
using System.Linq;
using Rainbow.DomainDriven.ConsoleApp.Domain;

namespace Rainbow.DomainDriven.ConsoleApp.CommandExecutor
{
    public class UserHandler :
        ICommandHandler<CreateUserCommand>,
        ICommandHandler<ModifyUserNameCommand>,
        ICommandHandler<ModifyUserSexCommand>,
        ICommandHandler<RemoveUserCommand>
    {
        // private readonly UserService _userService;
        // public UserHandler(UserService userService)
        // {
        //     this._userService = userService;
        // }

        public void Handler(ICommandContext context, RemoveUserCommand cmd)
        {
            //context.Get<User>(cmd.UserId).Dispose();
        }

        public void Handler(ICommandContext context, ModifyUserSexCommand cmd)
        {
            context.Get<User>(cmd.UserId).ModifySex(cmd.Sex);
        }

        public void Handler(ICommandContext context, ModifyUserNameCommand cmd)
        {
            //var repo = context.Repo<UserRepository>();
            //var isExists = repo.Exists(cmd.Name);
            //if (isExists)
            //    throw new DomainException(1, "已存在该名称的用户，请更换一个新的名称");

            context.Get<User>(cmd.UserId).ModifyName(cmd.Name);
        }

        public void Handler(ICommandContext context, CreateUserCommand cmd)
        {
            var user = new User(cmd.Id, cmd.Name, cmd.Sex);
            //_userService.RegisterUser(user);

            context.Add(user);
        }
    }



}
