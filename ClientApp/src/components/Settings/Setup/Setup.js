import React from "react";
import YoutubeDl from "./YoutubeDl";
import TwitchAuthentication from "./TwitchAuthentication";
import Path from "./Path";


export default function Setup() {
    return (
        <div>
            <TwitchAuthentication />
            <YoutubeDl/>
            <Path/>
        </div>
    )
}