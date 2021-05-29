import {Button, Dialog, DialogContent, DialogTitle} from "@material-ui/core";
import React, {useState} from "react";
import SearchAddSettingsQuality from "./SearchAddSettingsQuality";
import SearchAddSettingsGetLive from "./SearchAddSettingsGetLive";

export default function SearchAddSettings(streamer) {

    function handleClickSettings() {
        streamer.setSettingsOpen(!streamer.settingsOpen);
    }

    return (
        <Dialog open={streamer.settingsOpen} onClose={handleClickSettings}>
            <DialogTitle>
                Add {streamer.streamer.displayName}
            </DialogTitle>
            <DialogContent dividers>
                <SearchAddSettingsQuality setQuality={streamer.setQuality} />
                <SearchAddSettingsGetLive setGetLive={streamer.setGetLive} />
                <Button variant={"contained"} color={"primary"}>Add {streamer.streamer.displayName}</Button>
            </DialogContent>
        </Dialog>
    )
}