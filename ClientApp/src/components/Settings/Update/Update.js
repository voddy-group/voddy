import React, {useEffect, useState} from "react";
import {Button, CircularProgress, Typography} from "@material-ui/core";
import moment from "moment";

export default function Update(match) {
    const [version, setVersion] = useState("");
    const [buttonText, setButtonText] = useState("Check for updates");
    const [buttonHref, setButtonHref] = useState(null);
    const [loading, setLoading] = useState(false);
    const [clicked, setClicked] = useState(false);

    useEffect(async () => {
        if (await getVersion()) {
            updateAvailable();
        }
    }, [])

    async function getVersion() {
        const request = await fetch('update/check', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.status === 200) {
            var response = await request.json();
            setVersion(response.currentVersion);
            return response.updateAvailable;
        }
    }

    async function buttonClick() {
        setLoading(true);
        if (!await getVersion()) {
            setButtonText("No update found");
            setLoading(false);
        } else {
            updateAvailable();
        }
    }

    function updateAvailable() {
        setButtonText("Update found! Click here to update.");
        setClicked(true);
        setButtonHref("https://github.com/voddy-group/voddy/releases/latest");
        setLoading(false);
    }

    return (
        <div>
            <Typography color={"primary"} variant={"h3"}>
                Updates
            </Typography>
            <Typography variant={"body1"}>Current Version: {version}</Typography>
            <Button color={"primary"} onClick={!clicked ? buttonClick : null} href={buttonHref}>{loading ?
                <CircularProgress/>
                :
                <Typography variant={"body1"}>
                    {buttonText}
                </Typography>
            }</Button>
        </div>
    )
}