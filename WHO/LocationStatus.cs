using System.Collections.Generic;
using Models;
using WHO.Extensions;

namespace WHO
{
    public class LocationStatus
    {
        private readonly List<string> _location;
        private readonly Dictionary<int, WhoAction> _currentActions;
        private readonly string _locationKey;

        public string LocationKey => this._locationKey;

        public List<string> Location => this._location;

        public int ActionCount => this._currentActions.Count;

        public LocationStatus(List<string> location)
        {
            // Copy the location variable
            this._location = location;
            this._locationKey = location.ToKey();
            this._currentActions = new();
        }

        public void AddAction(WhoAction action)
        {
            this._currentActions[action.Id] = action;
        }

        public void RemoveAction(int actionUUID)
        {
            if (this._currentActions.ContainsKey(actionUUID))
            {
                this._currentActions.Remove(actionUUID);
            }
        }

        public WhoAction? GetAction(int actionUUID)
        {
            if (this._currentActions.ContainsKey(actionUUID))
            {
                return this._currentActions[actionUUID];
            }
            return null;
        }

        public List<int> GetActionsOfType(string actionName)
        {
            List<int> actions = new();
            foreach (var (uuid, action) in this._currentActions)
            {
                if (action.Parameters?.ActionName == actionName)
                {
                    actions.Add(uuid);
                }
            }
            return actions;
        }
    }
}
