namespace McServerControlAPI.Models
{
    public class MessageToStatusConverter
    {
        //The messages we're looking for should be as unique as possible, for example the server outputs "Done" when finished loading,
        //but other mods might also output "Done" before that. "Preparing spawn area" is the most consistent message
        //for determining loading is finished.

        private const string Done = "Preparing spawn area:";

        public void CheckMessageForStatusUpdate(string message, ref ServerStatus status)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (message.Contains(Done))
            {
                status = ServerStatus.Online;
                return;
            }
        }
    }
}
