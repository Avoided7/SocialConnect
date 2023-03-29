using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Interfaces;

namespace SocialConnect.WebUI.TagHelpers;

[HtmlTargetElement("friends-request")]
public class FriendTagHelper : TagHelper
{
    private readonly IFriendRepository _friendRepository;

    public string UserId { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;

    public FriendTagHelper(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        int count = await _friendRepository.GetFriendRequestsCountAsync(UserId);

        if (count == 0)
        {
            output.SuppressOutput();
            return;
        }
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.TagName = "span";
        output.AddClass("badge", HtmlEncoder.Default);
        output.AddClass("bg-success", HtmlEncoder.Default);
        output.AddClass("rounded-pill", HtmlEncoder.Default);
        output.AddClass("align-middle", HtmlEncoder.Default);
    
        output.Attributes.SetAttribute("style", Style);
        output.Content.SetContent(count.ToString());
    }
}