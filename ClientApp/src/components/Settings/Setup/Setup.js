import React from "react";
import YoutubeDl from "./YoutubeDl";
import TwitchAuthentication from "./TwitchAuthentication";


export default function Setup() {
    return (
        <div>
            <TwitchAuthentication />
            <YoutubeDl/>
        </div>
    )
}