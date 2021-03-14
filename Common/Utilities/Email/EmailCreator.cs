using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Utilities.Email
{
    public class EmailCreator
    {
        public static async Task<string> Create(EmailCreatorModel model, string rootPath, CancellationToken cancellationToken)
        {
            var body = await File.ReadAllTextAsync(Path.Combine(rootPath, "EmailSample.html"), Encoding.UTF8, cancellationToken);
            body = body.Replace("@Model.Name", model.Name);
            body = body.Replace("@Model.BodyText", model.BodyText);
            body = body.Replace("@Model.Link", model.Link);

            return body;
        }
    }

    public class EmailCreatorModel
    {
        public string Name { get; set; }
        public string BodyText { get; set; }
        public string Link { get; set; }
    }
}
