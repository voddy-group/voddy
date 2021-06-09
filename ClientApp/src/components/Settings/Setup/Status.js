import React, {useEffect, useState} from "react";
import {Typography} from "@material-ui/core";
import moment from "moment";

export default function Status() {
    const [uptime, setUptime] = useState(new moment.duration());    
    const [version, setVersion] = useState("");
    const [contentRootPath, setContentRootPath] = useState("");

    useEffect(() => {
        getUptime();
    }, []);
    
    useEffect(() => {
        const interval = setInterval(() => setUptime(new moment.duration(uptime).add(1, "second")), 1000);
        return () => {
            clearInterval(interval);
        };
    }, [uptime]);
    
    async function getUptime() {
        const request = await fetch('status', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.status === 200) {
            var response = await request.json();
            setUptime(new moment.duration(response.uptime));
            setVersion(response.version);
            setContentRootPath(response.contentRootPath);
        }
    }
    
    return (
        <div>
            <Typography color={"primary"} variant={"h3"}>
                Status
            </Typography>
            <Typography variant={"body1"}>
                Uptime: {uptime.asDays().toFixed(0)}d {uptime.hours() < 10 ? "0" + uptime.hours() : uptime.hours()}h {uptime.minutes() < 10 ? "0" + uptime.minutes() : uptime.minutes()}m {uptime.seconds() < 10 ? "0" + uptime.seconds() : uptime.seconds()}s
            </Typography>
            <Typography variant={"body1"}>
                Content Root Path: {contentRootPath}
            </Typography>
            <Typography variant={"body1"}>
                Version: {version}
            </Typography>
        </div>
    )
}
