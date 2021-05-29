import {
    AppBar,
    createMuiTheme,
    fade,
    InputBase,
    makeStyles,
    MuiThemeProvider,
    SvgIcon,
    Toolbar
} from "@material-ui/core";
import React from "react";

const styles = makeStyles((theme) => ({
    inputRoot: {
        color: "inherit"
    },
    inputInput: {
        paddingTop: 10,
        paddingBottom: 10,
        // vertical padding + font size from searchIcon
        paddingLeft: `calc(1em + ${theme.spacing(4)}px)`,
        transition: theme.transitions.create('width'),
        width: '100%',
        backgroundColor: fade(theme.palette.common.white, 0.15),
        '&:hover': {
            backgroundColor: fade(theme.palette.common.white, 0.25),
        },
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
    searchIcon: {
        padding: theme.spacing(0, 2),
        height: '100%',
        position: 'absolute',
        pointerEvents: 'none',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
    },
}))

export default function SearchBar(search) {
    const classes = styles();

    function handleChangeSearchValue(e) {
        search.searchValue(e.target.value);
    }

    return (
        <AppBar elevation={0} position={"static"} style={{width: "100%"}}>
            <Toolbar style={{paddingLeft: 12}}>
                <div className={classes.search} style={{width: "100%"}}>
                    <div className={classes.searchIcon}>
                        <SvgIcon>
                            <path fill="currentColor"
                                  d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
                        </SvgIcon>
                    </div>
                    <InputBase placeholder={"Search..."} onChange={handleChangeSearchValue} style={{width: "100%"}}
                               classes={{root: classes.inputRoot, input: classes.inputInput}}/>
                </div>
            </Toolbar>
        </AppBar>
    )
}