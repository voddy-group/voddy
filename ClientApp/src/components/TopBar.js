import React, {useState, useEffect} from 'react';
import {HubConnectionBuilder, LogLevel} from "@microsoft/signalr";
import {AppBar, fade, InputBase, makeStyles, Toolbar, Typography} from "@material-ui/core";
import TopBarSearch from "./TopBarSearch";
import MainNotification from "./MainNotification";


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
    }
}))


export default function TopBar(props) {
    const [message, setMessage] = useState("");
    const classes = styles();
    
    return (
        <div className={classes.root}>
            <AppBar position="static">
                <Toolbar>
                    <Typography className={classes.title}>
                        Voddy
                    </Typography>
                    <MainNotification hubConnection={props.hubConnection}/>
                    <TopBarSearch />
                </Toolbar>
            </AppBar>
        </div>
    )
}