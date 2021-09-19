import {
    Box,
    Button,
    Collapse,
    IconButton,
    makeStyles,
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableRow
} from "@material-ui/core";
import {KeyboardArrowDown, KeyboardArrowUp} from "@material-ui/icons";
import React, {useState} from "react";

const styles = makeStyles({
    root: {
        '& > *': {
            borderBottom: 'unset',
        },
    }
});

export default function QuartzRows(props) {
    const [open, setOpen] = useState(false);
    const classes = styles();
    var status;

    function getNextRun(nextFire) {
        var returnString = "";
        if (nextFire.days != 0) {
            returnString += nextFire.days + " days ";
        }
        if (nextFire.hours != 0) {
            returnString += nextFire.hours + " hours ";
        }
        if (nextFire.minutes != 0) {
            returnString += nextFire.minutes + " minutes ";
        }
        if (nextFire.seconds != 0) {
            returnString += nextFire.seconds + " seconds";
        }

        return returnString;
    }
    
    async function executeJob(jobName) {
        const request = await fetch('quartz/executeJob', {
            method: 'post',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "name": jobName,
                "scheduler": props.row.name
            })
        });

        if (request.ok) {
            props.getQuartzData();
        }
    }

    switch (props.row.status) {
        case 1:
            status = "Running";
            break;
        case 2:
            status = "Standby";
            break;
        default:
            status = "Disabled";
            break;
    }

    return (
        <>
            <TableRow classes={classes.root}>
                <TableCell>
                    <IconButton aria-label={"expand row"} size={"small"} onClick={() => setOpen(!open)}>
                        {open ? <KeyboardArrowUp/> : <KeyboardArrowDown/>}
                    </IconButton>
                </TableCell>
                <TableCell component={"th"} align={"left"}>{props.row.name}</TableCell>
                <TableCell align={"left"}>{status}</TableCell>
            </TableRow>
            <TableRow>
                <TableCell style={{paddingBottom: 0, paddingTop: 0}} colSpan={6}>
                    <Collapse in={open} timeout={"auto"} unmountOnExit>
                        <Box margin={1}>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>Job Name</TableCell>
                                        <TableCell>Cron</TableCell>
                                        <TableCell>Next Run</TableCell>
                                        <TableCell>Last Run</TableCell>
                                        <TableCell>Run</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {props.row.jobs.map((job) => (
                                        <TableRow key={job.name}>
                                            <TableCell width={"20%"}>{job.name}</TableCell>
                                            <TableCell width={"20%"}>{job.cron ? job.cron : "N/A"}</TableCell>
                                            <TableCell width={"20%"}>{job.nextFire.ticks !== 0 ? getNextRun(job.nextFire) : "N/A"}</TableCell>
                                            <TableCell width={"20%"}>{job.lastFireDateTime ? new Date(job.lastFireDateTime).toLocaleString() : "N/A"}</TableCell>
                                            <TableCell width={"20%"}><Button onClick={() => {executeJob(job.name)}}>Run Now</Button></TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    )
}