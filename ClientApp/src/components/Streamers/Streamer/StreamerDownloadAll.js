import {CircularProgress, IconButton, SvgIcon} from "@material-ui/core";
import React, {useEffect, useState} from "react";

export default function StreamerDownloadAll(streams) {
    const [buttonColour, setButtonColour] = useState("black");
    const [buttonDisabled, setButtonDisabled] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        var allDownloaded = true;
        for (var x = 0; x < streams.streams.length; x++) {
            if (!streams.streams[x].alreadyAdded) {
                allDownloaded = false;
                break;
            }
        }

        if (allDownloaded) {
            downloaded();
        } else {
            notDownloaded();
        }
    }, [streams.streams])

    async function DownloadStreams() {
        setIsLoading(true);
        var requestBody = {"data": streams.streams}
        const request = await fetch('backgroundTask/downloadStreams',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody) //TODO dont like this
            });

        if (request.ok) {
            downloaded();
            setIsLoading(false);
            var newStreams = [...streams.streams];
            for (var x = 0; x < newStreams.length; x++) {
                newStreams[x].downloading = true;
                newStreams[x].alreadyAdded = true;
            }
            streams.setStreams(newStreams);
        }
    }

    function downloaded() {
        setButtonColour("lightGray");
        setButtonDisabled(true);
    }

    function notDownloaded() {
        setButtonColour("black");
        setButtonDisabled(false);
    }

    return (
        <IconButton disabled={buttonDisabled} onClick={DownloadStreams}>
            <div hidden={isLoading}>
                <SvgIcon>
                    <path fill={buttonColour}
                          d="M9,1V7H5L12,14L19,7H15V1H9M5,16V18H19V16H5M5,20V22H19V20H5Z"/>
                </SvgIcon>
            </div>
            <CircularProgress hidden={!isLoading}/>
        </IconButton>
    )
}