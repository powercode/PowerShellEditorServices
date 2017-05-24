//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.PowerShell.EditorServices.Protocol.MessageProtocol;

namespace Microsoft.PowerShell.EditorServices.Protocol.LanguageServer
{
    public class GetProfilePathsRequest
    {
        public static readonly
            RequestType<object, GetProfilePathsResponse, object, object> Type =
                RequestType<object, GetProfilePathsResponse, object, object>.Create("powerShell/getProfilePaths");
    }

    public class GetProfilePathsResponse
    {
        public string CurrentUserAllHosts { get; set; }

        public string CurrentUserCurrentHost { get; set; }
    }
}

