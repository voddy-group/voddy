import {Button, Dialog, DialogContent, DialogTitle} from "@material-ui/core";
import React, {useState} from "react";
import SearchAddSettingsQuality from "./SearchAddSettingsQuality";
import SearchAddSettingsGetLive from "./SearchAddSettingsGetLive";
import {useHistory} from 'react-router-dom';

export default function SearchAddSettings(streamer) {
    let history = useHistory();
    const [getLive, setGetLive] = useState(false);
    const [quality, setQuality] = useState({"resolution":0,"fps":0});

    function handleClickSettings() {
        streamer.setSettingsOpen(!streamer.settingsOpen);
    }

    async function handleAddButtonClick() {
        var body = {
            "streamerId": streamer.streamer.streamerId,
            "displayName": streamer.streamer.displayName,
            "username": streamer.streamer.username,
            "thumbnailUrl": streamer.streamer.thumbnailLocation,
            "getLive": getLive,
            "quality": quality
        }
        
        const request = await fetch('database/streamer' +
            '?isNew=true',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });

        if (request.ok) {
            var response = await request.json();
            history.push("/streamer/" + response.id)
        }
    }

    return (
        <Dialog open={streamer.settingsOpen} onClose={handleClickSettings}>
            <DialogTitle>
                Add {streamer.streamer.displayName}
            </DialogTitle>
            <DialogContent dividers>
                <SearchAddSettingsQuality setQuality={setQuality} />
                <SearchAddSettingsGetLive setGetLive={setGetLive} />
                    <Button variant={"contained"} color={"primary"} onClick={handleAddButtonClick}>
                        Add {streamer.streamer.displayName}</Button>
            </DialogContent>
        </Dialog>
    )
}