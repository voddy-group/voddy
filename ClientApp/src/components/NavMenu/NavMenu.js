import React, {useState, useEffect} from 'react';
import {Link} from 'react-router-dom';
import './NavMenu.css';
import {List, ListItem, ListItemText, Collapse, makeStyles, Box, Typography} from "@material-ui/core";
import {createMuiTheme, ThemeProvider} from "@material-ui/core";
import {Info} from "@material-ui/icons";
import Notifications from "../Notifications";
import {HubConnection} from "@microsoft/signalr";
import NavMenuUpdateNotification from "./NavMenuUpdateNotification";
import WarningIcon from "@material-ui/icons/Warning";

const theme = createMuiTheme({
    overrides: {
        MuiList: {
            root: {
                position: "fixed"
            }
        }
    }
})

const styles = makeStyles((theme) => ({
    root: {
        height: "100%",
        width: "10%",
        position: "fixed"
    },
    nested: {
        width: "10%",
        paddingLeft: theme.spacing(2)
    }
}))

export default function NavMenu(props) {
    const [message, setMessage] = useState("");
    const classes = styles();
    


    return (
        <ThemeProvider theme={theme}>
            <List component="nav" className={classes.root}>
                <ListItem button component={Link} to="/">
                    <ListItemText primary="Streamers"/>
                </ListItem>
                <ListItem button component={Link} to="/search">
                    <ListItemText primary="Search"/>
                </ListItem>
            <NavMenuUpdateNotification hubConnection={props.hubConnection} />
            </List>
            <Notifications hubConnection={props.hubConnection} />
        </ThemeProvider>
    );
}
