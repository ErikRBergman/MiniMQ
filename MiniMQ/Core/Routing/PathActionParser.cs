namespace MiniMQ.Core.Routing
{
    using System;

    public class PathActionParser
    {
        /// <summary>
        /// The path action map.
        /// </summary>
        private readonly PathActionMapItem[] pathActionMap;

        public PathActionParser(PathActionMapItem[] pathActionMap)
        {
            this.pathActionMap = pathActionMap;

            if (this.pathActionMap.Length > 64)
            {
                throw new NotImplementedException("This implementation is limited to 64 action map items");
            }
        }

        private static PathActionMapItem PathActionUnknown = new PathActionMapItem(string.Empty, PathAction.Unknown);

        public PathActionMapItem GetPathAction(string path)
        {
            var pathActionCount = this.pathActionMap.Length;

            // The possible actions are kept as bits in this ulong
            ulong possibleActions = ulong.MaxValue;

            var pathLength = path.Length;

            for (var pathCharacterIndex = 0; pathCharacterIndex < pathLength; pathCharacterIndex++)
            {
                ulong possibleActionsIndexer = 1;

                for (var pathMapItemIndex = 0; pathMapItemIndex < pathActionCount; pathMapItemIndex++)
                {
                    if ((possibleActions & possibleActionsIndexer) != 0)
                    {
                        var mapPathLength = this.pathActionMap[pathMapItemIndex].Path.Length;

                        if (pathCharacterIndex < mapPathLength)
                        {
                            if (path[pathCharacterIndex] == this.pathActionMap[pathMapItemIndex].Path[pathCharacterIndex])
                            {
                                if (pathCharacterIndex == mapPathLength - 1)
                                {
                                    return this.pathActionMap[pathMapItemIndex];
                                }
                            }
                            else
                            {
                                // No longer possible action. Xor away the possibility
                                possibleActions = possibleActions ^ possibleActionsIndexer;
                            }
                        }
                    }

                    possibleActionsIndexer <<= 1;
                }
            }

            return PathActionUnknown;
        }
    }
}