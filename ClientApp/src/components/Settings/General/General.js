import React, {useState} from "react";
import StreamQuality from "../Setup/StreamQuality";
import WorkerCount from "../Setup/WorkerCount";
import {Accordion, AccordionDetails, AccordionSummary, Box, makeStyles, Typography} from "@material-ui/core";

const styles = makeStyles((theme) => ({
    accordionRoot: {
        border: '1px solid rgba(0, 0, 0, .125)',
        boxShadow: 'none',
        '&:not(:last-child)': {
            borderBottom: 0,
        },
        '&:before': {
            display: 'none',
        },
        '&$expanded': {
            margin: '0',
        },
    },
    expanded: {
        margin: "0"
    }
}))

export default function General() {
    const classes = styles();
    return (
        <div>
            <StreamQuality/>
            <br/>
            <Accordion className={classes.accordionRoot} classes={{expanded: classes.expanded}}>
                <AccordionSummary>
                    <Typography>Show Advanced Settings</Typography>
                </AccordionSummary>
                <AccordionDetails>
                    <div>
                        <WorkerCount/>
                        <h2>Background Job Page</h2>
                        <a href="/hangfire">We use Hangfire to queue background jobs.</a>
                        <p>This controls 99% of the functions of voddy. Unless you know what you are doing, you should
                            not adjust any settings in that area. Doing so may break your current running instance. If
                            you have adjusted any jobs, and wish to revert the changes, restart the application.</p>
                    </div>
                </AccordionDetails>
            </Accordion>
        </div>
    )
}
