namespace Server.Models
{
    public enum ClientSessionState
    {
        Disconnected, 
        Connected, 
        AwaitingAuth, 
        Authenticated, 
        ReceivingFile, 
        SendingFile, 
        Blocked, 
        Terminated
    }
}
