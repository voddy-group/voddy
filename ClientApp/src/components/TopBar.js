import React, {useState, useEffect} from 'react';
import {HubConnectionBuilder, LogLevel} from "@microsoft/signalr";
import {AppBar, fade, InputBase, makeStyles, Toolbar, Typography} from "@material-ui/core";
import SearchIcon from '@material-ui/icons/Search'


const styles = makeStyles((theme) => ({
    root: {
        flexGrow: 1
    },
    title: {
        flexGrow: 1,
        display: "none",
        [theme.breakpoints.up('sm')]: {
            display: "block"
        }
    },
    search: {
        position: "relative",
        marginLeft: 0,
        borderRadius: theme.shape.borderRadius,
        backgroundColor: fade(theme.palette.common.white, 0.15),
        '&:hover': {
            backgroundColor: fade(theme.palette.common.white, 0.25),
        },
        [theme.breakpoints.up('sm')]: {
            marginLeft: theme.spacing(1),
            width: 'auto',
        },
    },
    inputRoot: {
        color: "inherit"
    },
    inputInterior: {
        padding: theme.spacing(1, 1, 1, 0),
        paddingLeft: 'calc(1em + ${theme.spacing(4)}px',
        transition: theme.transitions.create('width'),
        width: "100%",
        [theme.breakpoints.up('sm')]: {
            width: '12ch',
            '&:focus': {
                width: '20ch',
            },
        }
    }
}))


export default function TopBar() {
    const [message, setMessage] = useState("");
    const classes = styles();

    useEffect(() => {
        const hubConnection = new HubConnectionBuilder().withUrl('/notificationhub')
            .configureLogging(LogLevel.Information)
            .build();

        async function start() {
            try {
                await hubConnection.start();
                console.log("SignalR Connected.");
            } catch (err) {
                console.log(err);
                setTimeout(start, 5000);
            }
        }

        hubConnection.onclose(start);
        start();

        hubConnection.on("ReceiveMessage", (message) => {
            setMessage(message);
        })
    })
    
    return (
        <div className={classes.root}>
            <AppBar position="static">
                <Toolbar>
                    <Typography className={classes.title}>
                        Voddy
                    </Typography>
                    <div className={classes.search}>
                        <InputBase placeholder="Search" classes={{root: classes.inputRoot, input: classes.inputInterior}}/>
                    </div>
                </Toolbar>
            </AppBar>
        </div>
    )
}