namespace MiniMQ.Core.Routing
{
    public struct PathActionMapItem
    {
        public PathActionMapItem(string path, PathAction pathAction)
        {
            this.Path = path;
            this.PathAction = pathAction;
        }

        public string Path;

        public PathAction PathAction;
    }
}