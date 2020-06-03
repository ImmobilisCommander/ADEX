using System;
using System.Collections.Generic;

namespace Adex.Common
{
    public interface ICsvLoader
    {
        event EventHandler<MessageEventArgs> OnMessage;

        void LoadLinks(string path, Dictionary<string, IEntity> companies, Dictionary<string, IEntity> beneficiaries, Dictionary<string, ILink> links);

        void LoadProviders(string path, Dictionary<string, IEntity> entities);
    }
}