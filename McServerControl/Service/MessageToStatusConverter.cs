namespace McServerControlAPI.Models
{
    public class MessageToStatusConverter
    {
        //The messages we're looking for should be as unique as possible, for example the server outputs "Done" when finished loading,
        //but other mods might also output "Done" before that.

        private const string Done = "Time elapsed:";

        private const string Stopped = "Stopping the server";


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

            if (message.Contains(Stopped))
            {
                status = ServerStatus.Offline;
                return;
            }
        }
    }
}
