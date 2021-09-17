import React, {useState, useEffect} from "react";
import {
    AppBar,
    Box, Button,
    Collapse,
    IconButton, makeStyles, Paper,
    Table,
    TableBody,
    TableCell, TableContainer,
    TableHead,
    TableRow, Toolbar,
    Typography
} from "@material-ui/core";
import {KeyboardArrowDown, KeyboardArrowUp, Refresh} from "@material-ui/icons";
import QuartzRows from "./QuartzRows";

const styles = makeStyles({
    grow: {
        flexGrow: 1
    },
});

export default function Quartz() {
    const [schedulers, setSchedulers] = useState([]);
    const classes = styles();

    useEffect(() => {
        getQuartzData();
    }, [])

    async function getQuartzData() {
        const request = await fetch('quartz/schedulers', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.ok) {
            var response = await request.json();
            setSchedulers(response.schedulers);
        }
    }

    return (
        <div>
            <AppBar position="static" elevation={0} style={{backgroundColor: "unset"}}>
                <Toolbar>
                    <div>
                        <Typography variant={"h2"}>Quartz Scheduler</Typography>
                        <Typography>The service that organizes background jobs. Do not execute the jobs manually unless
                            necessary,
                            as many executions over a short period of time could cause Twitch API issues. </Typography>
                    </div>
                    <div className={classes.grow}/>
                    <IconButton><Refresh onClick={() => {
                        getQuartzData()
                    }}/></IconButton>
                </Toolbar>
            </AppBar>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell/>
                            <TableCell align={"left"}>Scheduler Name</TableCell>
                            <TableCell align={"left"}>Enabled</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {schedulers.length > 0 ?
                            schedulers.map((row) => (
                                <QuartzRows key={row.name} row={row}/>
                            ))
                            :
                            null}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    )
}