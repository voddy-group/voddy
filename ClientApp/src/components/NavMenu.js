import React, {useState, useEffect} from 'react';
import {Link} from 'react-router-dom';
import './NavMenu.css';
import {List, ListItem, ListItemText, Collapse, makeStyles} from "@material-ui/core";
import {createMuiTheme, ThemeProvider} from "@material-ui/core";

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

export default function NavMenu() {
    const [message, setMessage] = useState("");
    const [dropDown, setDropDown] = useState(false);
    const classes = styles();

    function toggleDropDown() {
        setDropDown(prevState => !prevState);
    }

    return (
        <ThemeProvider theme={theme}>
            <List component="nav" className={classes.root}>
                <ListItem button component={Link} to="/">
                    <ListItemText primary="Streamers"/>
                </ListItem>
                <ListItem button component={Link} to="/search">
                    <ListItemText primary="Search"/>
                </ListItem>
                <ListItem button onClick={toggleDropDown}>
                    <ListItemText primary="Settings"/>
                </ListItem>
                <Collapse in={dropDown} timeout="auto" unmountOnExit>
                    <List className={classes.root}>
                        <ListItem button component={Link} to="/settings/general">
                            <ListItemText className={classes.nested} primary="General"/>
                        </ListItem>
                        <ListItem button component={Link} to="/settings/setup">
                            <ListItemText className={classes.nested} primary="Setup"/>
                        </ListItem>
                    </List>
                </Collapse>
            </List>
        </ThemeProvider>
    );
}
