import React, {useEffect, useState} from "react";
import {Button, CircularProgress, Typography} from "@material-ui/core";
import moment from "moment";
import WarningIcon from "@material-ui/icons/Warning";

export default function Update(match) {
    const [currentVersion, setCurrentVersion] = useState("");
    const [latestVersion, setLatestVersion] = useState("");
    const [buttonHref, setButtonHref] = useState(null);
    const [loading, setLoading] = useState(true);
    const [updateButtonDisabled, setUpdateButtonDisabled] = useState(true);
    const [buttonText, setButtonText] = useState("No update found.");

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

        if (request.ok) {
            var response = await request.json();
            setCurrentVersion(response.currentVersion);
            setLatestVersion(response.latestVersion)
            if (response.updateAvailable) {
                updateAvailable();
            } else {
                setLoading(false);
            }
        }
    }

    function updateAvailable() {
        setButtonText("Update found! Click here to update.");
        setUpdateButtonDisabled(false);
        setButtonHref("https://github.com/voddy-group/voddy/releases/latest");
        setLoading(false);
    }

    return (
        <div>
            <Typography color={"primary"} variant={"h3"}>
                Updates
            </Typography>
            <Typography variant={"body1"}>Latest Version: {latestVersion}</Typography>
            <Button color={"primary"} disabled={updateButtonDisabled} href={buttonHref}>{loading ?
                <CircularProgress/>
                :
                <Typography variant={"body1"}>
                    {!updateButtonDisabled ?
                        <WarningIcon color={"secondary"}/>
                        :
                        null}
                    {buttonText}
                </Typography>
            }</Button>
        </div>
    )
}