import {CircularProgress, IconButton, SvgIcon} from "@material-ui/core";
import React, {useEffect, useState} from "react";

export default function StreamerDownloadAll(props) {
    const [buttonColour, setButtonColour] = useState("black");
    const [buttonDisabled, setButtonDisabled] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        var allDownloaded = true;
        for (var x = 0; x < props.streams.length; x++) {
            if (!props.streams[x].alreadyAdded) {
                allDownloaded = false;
                break;
            }
        }

        if (allDownloaded) {
            downloaded();
        } else {
            notDownloaded();
        }
    }, [props.streams])

    async function DownloadStreams() {
        setIsLoading(true);
        const request = await fetch('backgroundTask/downloadStreams?id=' + props.streamerId,
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (request.ok) {
            downloaded();
            setIsLoading(false);
            var newStreams = [...props.streams];
            for (var x = 0; x < newStreams.length; x++) {
                newStreams[x].downloading = true;
                newStreams[x].alreadyAdded = true;
            }
            props.setStreams(newStreams);
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