using System;
using System.Collections.Generic;

namespace Adex.Common
{
    public interface ICsvLoader
    {
        event EventHandler<MessageEventArgs> OnMessage;

        void LoadReferences();

        void LoadProviders(string path);

        void LoadLinks(string path);

        string LinksToJson();
    }
}