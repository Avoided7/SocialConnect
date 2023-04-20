using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.WebUI.TagHelpers;

[HtmlTargetElement("chat-messages")]
public class ChatTagHelper : TagHelper
{
    private readonly IChatRepository _chatRepository;
    public string UserId { get; set; } = string.Empty;

    public ChatTagHelper(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        IReadOnlyCollection<Chat> chats = await _chatRepository.GetAsync(chat =>
            chat.Users.Any(user => user.UserId == UserId) &&
            chat.Messages.Any(message =>
                message.Views.All(view =>
                    view.UserId != UserId)));
        int chatsCount = chats.Count();
        if (chatsCount == 0)
        {
            output.Attributes.SetAttribute("hidden", "hidden");
        }

        output.Attributes.SetAttribute("id", "chats");

        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.TagName = "span";
        output.AddClass("badge", HtmlEncoder.Default);
        output.AddClass("bg-warning", HtmlEncoder.Default);
        output.AddClass("rounded-pill", HtmlEncoder.Default);
        output.AddClass("align-middle", HtmlEncoder.Default);
        output.Content.SetContent(chatsCount.ToString());
    }
}