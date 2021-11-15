import React from "react";
import YtDlp from "./YtDlp";
import TwitchAuthentication from "./TwitchAuthentication";


export default function Setup() {
    return (
        <div>
            <TwitchAuthentication />
            <YtDlp/>
        </div>
    )
}