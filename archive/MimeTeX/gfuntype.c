/****************************************************************************
 *
 * Copyright (c) 2002, John Forkosh Associates, Inc.  All rights reserved.
 * --------------------------------------------------------------------------
 * This file is part of mimeTeX, which is free software. You may redistribute
 * and/or modify it under the terms of the GNU General Public License,
 * version 2 or later, as published by the Free Software Foundation.
 *      MimeTeX is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY, not even the implied warranty of MERCHANTABILITY.
 * See the GNU General Public License for specific details.
 *      By using mimeTeX, you warrant that you have read, understood and
 * agreed to these terms and conditions, and that you are at least 18 years
 * of age and possess the legal right and ability to enter into this
 * agreement and to use mimeTeX in accordance with it.
 *      Your mimeTeX distribution should contain a copy of the GNU General
 * Public License.  If not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------
 *
 * Program:	gfuntype  [-n fontname]  [-m msglevel]  [infile [outfile]]
 *
 * Purpose:	Parses output from  gftype -i
 *		and writes pixel bitmap data of the characters
 *		in a format suitable for a C header file, etc.
 *
 * --------------------------------------------------------------------------
 *
 * Command-line Arguments:
 *		--- args can be in any order ---
 *		infile		name of input file
 *				(defaults to stdin if no filenames given)
 *		outfile		name of output file
 *				(defaults to stdout if <2 filenames given)
 *		-m msglevel	verbose if msglevel>=9 (vv if >=99)
 *		-n fontname	string used for fontname
 *				(defaults to noname)
 *
 * Exits:	0=success,  1=some error
 *
 * Notes:     o	To compile
 *		cc gfuntype.c mimetex.c -lm -o gfuntype
 *		needs mimetex.c and mimetex.h
 *
 * Source:	gfuntype.c
 *
 * --------------------------------------------------------------------------
 * Revision History:
 * 09/22/02	J.Forkosh	Installation.
 *
 ****************************************************************************/

/* --------------------------------------------------------------------------
standard headers, program parameters, global data and macros
-------------------------------------------------------------------------- */
/* --- standard headers --- */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
/* --- application headers --- */
/* #define SIGNEDCHAR */
#include "mimetex.h"
/* --- parameters either -D defined on cc line, or defaulted here --- */
#ifndef	MSGLEVEL
#define	MSGLEVEL 0
#endif
/* --- message level (verbose test) --- */
static	int msglevel = MSGLEVEL;	/* verbose if msglevel >= 9 */
static	FILE *msgfp;			/* verbose output goes here */
/* --- miscellaneous other data --- */
#define	CORNER_STUB ".<--"		/* start of upper,lower-left line */
#define	TYPECAST    "(pixbyte *)"	/* typecast for pixmap string */

/* ==========================================================================
 * Function:	main() for gfuntype.c
 * Purpose:	interprets command-line args, etc
 * --------------------------------------------------------------------------
 * Command-Line Arguments:
 *		See above
 * --------------------------------------------------------------------------
 * Returns:	0=success, 1=some error
 * --------------------------------------------------------------------------
 * Notes:     o
 * ======================================================================= */
/* --- entry point --- */
int	main ( int argc, char *argv[] )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
int	argnum = 0;		/* argv[] index for command-line args */
int	inarg=0, outarg=0;	/* argv[] indexes for infile, outfile */
int	iserror = 1;		/* error signal */
int	charnum,		/* character number (nextchar->charnum) */
	nchars = 0;		/* #chars in font */
char	fontname[99] = "noname", /* font name */
	*getcharname();		/* get character name from its number */
FILE	/* *fopen(),*/ *infp=stdin, *outfp=stdout; /* init file pointers */
chardef	*getnextchar(), *nextchar, /* read and parse next char in infp */
	*fontdef[256];		/* chars stored using charnum as index */
int	cstruct_chardef();	/* emit C struct for a character map */
int	type_raster();		/* display debugging output */
char	*copyright =		/* copyright, gnu/gpl notice */
 "+-----------------------------------------------------------------------+\n"
 "|gfuntype ver 1.00, Copyright(c) 2002-2003, John Forkosh Associates, Inc|\n"
 "+-----------------------------------------------------------------------+\n"
 "| gfuntype is free software licensed to you under terms of the GNU/GPL, |\n"
 "|           and comes with absolutely no warranty whatsoever.           |\n"
 "+-----------------------------------------------------------------------+";
/* --------------------------------------------------------------------------
interpret command-line arguments
-------------------------------------------------------------------------- */
while ( argc > ++argnum )	/* check for flags and filenames */
    if ( *argv[argnum] == '-' )	/* got some '-' flag */
      {
      char flag = tolower(*(argv[argnum]+1)); /* char following '-' */
      argnum++;			/* arg following flag is usually its value */
      switch ( flag )		/* see what user wants to tell us */
	{
	/* --- no usage for clueless users yet --- */
	default:  exit(iserror); /* exit quietly for unrecognized input */
	/* --- adjustable program parameters (not checking input) --- */
	case 'm': msglevel   = atoi(argv[argnum]); break;
	case 'n': strcpy(fontname,argv[argnum]); break;
	} /* --- end-of-switch() --- */
      } /* --- end-of-if(*argv[]=='-') --- */
    else			/* this arg not a -flag, so it must be... */
      if ( inarg == 0 )		/* no infile arg yet */
	inarg = argnum;		/* so use this one */
      else			/* we already have an infile arg */
	if ( outarg == 0 )	/* but no outfile arg yet */
	  outarg = argnum;	/* so use this one */
/* --- set verbose file ptr --- */
msgfp = (outarg>0? stdout : stderr); /* use stdout or stderr */
/* --- emit copyright, gnu/gpl notice --- */
fprintf(msgfp,"%s\n",copyright); /* display copyright, gnu/gpl info */
/* --- display input args if verbose output --- */
if ( msglevel >= 9 )		/* verbose output requested */
  fprintf(msgfp,"gfuntype> infile=%s, outfile=%s, fontname=%s\n",
  (inarg>0?argv[inarg]:"stdin"), (outarg>0?argv[outarg]:"stdout"), fontname);
/* --------------------------------------------------------------------------
initialization
-------------------------------------------------------------------------- */
/* --- initialize font[] array --- */
for ( charnum=0; charnum<256; charnum++ ) /*for each possible char in font*/
  fontdef[charnum] = (chardef *)NULL;	/* char doesn't exist yet */
/* --- open input file (if necessary) --- */
if ( inarg > 0 )		/* input from file, not from stdin */
  if ( (infp = fopen(argv[inarg],"r")) == NULL ) /*try to open input file*/
    { fprintf(msgfp,"gfuntype> can't open %s for read\n",argv[inarg]);
      goto end_of_job; }	/* report error and quit */
/* --------------------------------------------------------------------------
process input file
-------------------------------------------------------------------------- */
while ( (nextchar=getnextchar(infp)) != NULL ) /* get each char in file */
  {
  /* --- display character info --- */
  if ( msglevel >= 9 )			/* verbose output requested */
    fprintf(msgfp,"gfuntype> Char#%3d, loc %4d: ul=(%d,%d) ll=(%d,%d)\n",
    nextchar->charnum, nextchar->location,
    nextchar->topleftcol,nextchar->toprow,
    nextchar->botleftcol,nextchar->botrow);
  if ( msglevel >= 19 )			/* if a bit more verbose */
    type_raster(&(nextchar->image),msgfp); /*display ascii image of raster*/
  /* --- store character in font */
  charnum = nextchar->charnum;		/* get char number of char in font */
  if ( charnum>=0 && charnum<=255 )	/* check for valid range */
    fontdef[charnum] = nextchar;	/* store char in font */
  } /* --- end-of-while(charnum>0) --- */
/* --------------------------------------------------------------------------
generate output file
-------------------------------------------------------------------------- */
/* --- open output file (if necessary) --- */
if ( outarg > 0 )		/* output to a file, not to stdout */
  if ( (outfp = fopen(argv[outarg],"w")) == NULL ) /*try to open output file*/
    { fprintf(msgfp,"gfuntype> can't open %s for write\n",argv[outarg]);
      goto end_of_job; }	/* report error and quit */
/* --- header lines --- */
fprintf(outfp,"/%c --- fontdef for %s --- %c/\n", '*',fontname,'*');
fprintf(outfp,"static\tchardef %c%s[] =\n   {\n", ' ',fontname);
/* --- write characters comprising font --- */
for ( charnum=0; charnum<256; charnum++ ) /*for each possible char in font*/
  if ( fontdef[charnum] != (chardef *)NULL ) /*check if char exists in font*/
    { if ( ++nchars > 1 )		/* bump count */
	fprintf(outfp,",\n");		/* and terminate preceding chardef */
      fprintf(outfp,"      /%c --- pixel bitmap for %s char#%d %s --- %c/\n",
	'*',fontname,charnum,getcharname(fontname,charnum),'*');
      cstruct_chardef(fontdef[charnum],outfp,6); } /*emit chardef as struct*/
  else
      if(0)fprintf(outfp,"NULL");	/* no character in this position */
/* --- write trailer chardef and closing brace --- */
fprintf(outfp,",\n");			/* finish up last map from loop */
fprintf(outfp,"      /%c --- trailer  --- %c/\n",'*','*'); /* trailer... */
fprintf(outfp,"      { -99, -999,  0,0,0,0, { 0,0,0, %s\"\\0\" }  }\n",
     TYPECAST);
fprintf(outfp,"   } ;\n");		/* terminating }; for fontdef */
/* --------------------------------------------------------------------------
end-of-job
-------------------------------------------------------------------------- */
/* --- reset error status for okay exit --- */
iserror = 0;
/* --- close files (if they're open and not stdin/out) --- */
end_of_job:
  if (  infp!=NULL &&  infp!=stdin  ) fclose( infp);
  if ( outfp!=NULL && outfp!=stdout ) fclose(outfp);
exit ( iserror );
} /* --- end-of-function main() --- */


/* ==========================================================================
 * Function:	getnextchar ( fp )
 * Purpose:	Reads and parses the next character definition on fp,
 *		and returns a new chardef struct describing that character.
 * --------------------------------------------------------------------------
 * Arguments:	fp (I)		FILE *  to input file
 *				(containing output from  gftype -i)
 * Returns:	( chardef * )	ptr to chardef struct describing character,
 *				or NULL for eof or any error
 * --------------------------------------------------------------------------
 * Notes:     o	fp is left so the next line read from it will be
 *		the one following the final .<-- line.
 * ======================================================================= */
/* --- entry point --- */
chardef	*getnextchar ( FILE *fp )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
chardef	*new_chardef(), *nextchar=(chardef *)NULL; /*ptr returned to caller*/
int	delete_chardef();		/* free allocated memory if error */
int	findnextchar(), charnum,location; /* get header line for next char */
int	rasterizechar();		/* ascii image --> raster pixmap */
int	parsecorner();			/* get col,row from ".<--" line */
char	*readaline();			/* read next line from fp */
/* --------------------------------------------------------------------------
initialization
-------------------------------------------------------------------------- */
/* --- find and interpret header line for next character --- */
charnum = findnextchar(fp,&location);	/* read and parse header line */
if ( charnum < 0 ) goto error;		/* eof or error, no more chars */
/* --- allocate a new chardef struct and begin populating it --- */
if ( (nextchar=new_chardef())		/* allocate a new chardef */
==   (chardef *)NULL ) goto error;	/* and quit if we failed */
nextchar->charnum = charnum;		/* store charnum in struct */
nextchar->location = location;		/* and location */
/* --- get upper-left corner line --- */
if ( !parsecorner(readaline(fp),	/* parse corner line */
&(nextchar->toprow),&(nextchar->topleftcol)) ) /* row and col from line */
  goto error;				/* and quit if failed */
/* --------------------------------------------------------------------------
interpret character image (and parse terminating corner line)
-------------------------------------------------------------------------- */
/* --- read ascii character image and interpret as integer bitmap --- */
if ( rasterizechar(fp,&nextchar->image) != 1 ) /* parse image of char */
  goto error;				/* and quit if failed */
/* --- get lower-left corner line --- */
if ( !parsecorner(readaline(NULL),	/* reread and parse corner line */
&(nextchar->botrow),&(nextchar->botleftcol)) ) /* row and col from line */
  goto error;				/* and quit if failed */
/* --------------------------------------------------------------------------
done
-------------------------------------------------------------------------- */
goto end_of_job;			/* skip error return if successful */
error:
  if ( nextchar != (chardef *)NULL )	/* have an allocated chardef */
    delete_chardef(nextchar);		/* so deallocate it */
  nextchar = (chardef *)NULL;		/* and reset ptr to null for error */
end_of_job:
  return ( nextchar );			/* back with chardef or null */
} /* --- end-of-function getnextchar() --- */


/* ==========================================================================
 * Function:	getcharname ( fontname, charnum )
 * Purpose:	Looks up charnum for the family specified by fontname
 *		and returns the corresponding charname.
 * --------------------------------------------------------------------------
 * Arguments:	fontname (I)	char * containing fontname for font family
 *		charnum (I)	int containing the character number
 *				whose corresponding name is wanted.
 * Returns:	( char * )	ptr to character name
 *				or NULL if charnum not found in table
 * --------------------------------------------------------------------------
 * Notes:     o
 * ======================================================================= */
/* --- entry point --- */
char	*getcharname ( char *fontname, int charnum )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
/* --- recognized font family names and our corresponding numbers --- */
static	char *fnames[] = { "cmr","cmmi","cmsy","cmex",NULL };
static	int    fnums[] = { CMR10,CMMI10,CMSY10,CMEX10,  -1 };
static	char *noname = "(noname)";  /* char name returned if lookup fails */
/* --- other local declarations --- */
char	*charname = noname;	/* character name returned to caller */
char	flower[99] = "noname";	/* lowercase caller's fontname */
int	ifamily = 0,		/* fnames[] (and fnums[]) index */
	ichar = 0;
/* --------------------------------------------------------------------------
lowercase caller's fontname and look it up in fnames[]
-------------------------------------------------------------------------- */
/* --- lowercase caller's fontname --- */
for ( ichar=0; *fontname!='\000'; ichar++,fontname++ )/*lowercase each char*/
  flower[ichar] = (isalpha(*fontname)? tolower(*fontname) : *fontname);
flower[ichar] = '\000';		/* null-terminate lowercase fontname */
if ( strlen(flower) < 2 ) goto end_of_job; /* no lookup match possible */
/* --- look up lowercase fontname in our fnames[] table --- */
for ( ifamily=0; ;ifamily++ )	/* check fnames[] for flower */
  if ( fnames[ifamily] == NULL ) goto end_of_job; /* quit at end-of-table */
  else if ( strstr(flower,fnames[ifamily]) != NULL ) break; /* found it */
ifamily = fnums[ifamily];	/* xlate index to font family number */
/* --------------------------------------------------------------------------
now look up name for caller's charnum in ifamily, and return it to caller
-------------------------------------------------------------------------- */
/* --- search symtable[] for charnum in ifamily --- */
for ( ichar=0; ;ichar++ )	/*search symtable[] for charnum in ifamily*/
  if ( symtable[ichar].symbol == NULL ) goto end_of_job; /* end-of-table */
  else
    if ( symtable[ichar].family == ifamily /* found desired family */
    &&   symtable[ichar].handler == NULL ) /* and char isn't a "dummy" */
      if ( symtable[ichar].charnum == charnum ) break; /* found charnum */
/* --- return corresponding charname to caller --- */
charname = symtable[ichar].symbol; /* pointer to symbol name in table */
end_of_job:
  return ( charname );
} /* --- end-of-function getcharname() --- */


/* ==========================================================================
 * Function:	findnextchar ( fp, location )
 * Purpose:	Finds next "beginning of char" line in fp
 *		and returns the character number,
 *		and (optionally) location if arg provided.
 * --------------------------------------------------------------------------
 * Arguments:	fp (I)		FILE *  to input file
 *				(containing output from  gftype -i)
 *		location (O)	int *  returning "location" of character
 *				(or pass NULL and it won't be returned)
 * Returns:	( int )		character number,
 *				or -1 for eof or any error
 * --------------------------------------------------------------------------
 * Notes:     o	fp is left so the next line read from it will be
 *		the one following the "beginning of char" line
 * ======================================================================= */
/* --- entry point --- */
int	findnextchar ( FILE *fp, int *location )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
static	char keyword[99]="beginning of char "; /*signals start of next char*/
char	*readaline(), *line;	/* read next line from fp */
char	*strstr(), *strchr(), *delim; /* search line for substring, char */
char	token[99];		/* token extracted from line */
int	charnum = (-1);		/* character number returned to caller */
/* --------------------------------------------------------------------------
keep reading lines until eof or keyword found
-------------------------------------------------------------------------- */
while ( (line=readaline(fp)) != NULL ) /* read lines until eof */
  {
  if ( msglevel >= 999 )	/* very, very verbose output requested */
    fprintf(msgfp,"nextchar> line = %s\n",line);
  if ( (delim=strstr(line,keyword)) != NULL ) /* found keyword on line */
    {
    /* --- get character number from line --- */
    strcpy(token,delim+strlen(keyword)); /* char num follows keyword */
    charnum = atoi(token);	/* interpret token as integer charnum */
    /* --- get location at beginning of line --- */
    if ( location != (int *)NULL )  /* caller wants location returned */
      if ( (delim=strchr(line,':')) != NULL ) /* location precedes colon */
	{ *delim = '\000';	/* terminate line after location */
	  *location = atoi(line); } /* interpret location as integer */
    break;			/* back to caller with charnum */
    } /* --- end-of-if(delim!=NULL) --- */
  } /* --- end-of-while(line!=NULL) --- */
return ( charnum );		/* back to caller with char number or -1 */
} /* --- end-of-function findnextchar() --- */


/* ==========================================================================
 * Function:	rasterizechar ( fp, rp )
 * Purpose:	Reads and parses subsequent lines from fp
 *		(until a terminating ".<--" line),
 *		representing the ascii image of the character in fp,
 *		and returns the results in raster struct rp
 * --------------------------------------------------------------------------
 * Arguments:	fp (I)		FILE *  to input file
 *				(containing output from  gftype -i)
 *				positioned immediately after top .<-- line,
 *				ready to read first line of ascii image
 *		rp (O)		raster *  returning the rasterized
 *				character represented on fp as an ascii image
 * Returns:	( int )		1=okay, or 0=eof or any error
 * --------------------------------------------------------------------------
 * Notes:     o	fp is left so the last line (already) read from it
 *		contains the terminating .<-- corner information
 *		(readaline(NULL) will reread this last line)
 *	      o	char images on fp can be no wider than 31 pixels
 * ======================================================================= */
/* --- entry point --- */
int	rasterizechar ( FILE *fp, raster *image )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
char	*readaline(), *line;	/* read next scan line for char from fp */
unsigned char bitvec[512][64];	/* scan lines parsed (up to 512x512 bits) */
int	height = 0,		/* #scan lines in fp comprising char */
	width = 0,		/* #chars on longest scan line */
	pixsz = 1;		/* default #bits per pixel, 1=bitmap */
int	iscan, ipixel=0,	/* bitvec[] index, raster pixel map index */
	ibit;			/* bit along scan (i.e., 0...width-1) */
int	isokay = 0;		/* returned status, init for failure */
/* --------------------------------------------------------------------------
read lines till ".<--" terminator, and construct one vector[] int per line
-------------------------------------------------------------------------- */
while ( (line=readaline(fp)) != NULL ) /* read lines until eof */
  {
  /* --- allocations and declarations --- */
  int	icol, ncols=strlen(line); /* line[] column index, #cols in line[] */
  /* --- check for end-of-char (when we encounter corner line) --- */
  if ( memcmp(line,CORNER_STUB,strlen(CORNER_STUB)) == 0 ) /* corner line */
    break;			/* so done with loop */
  /* --- parse line (encode asterisks comprising character image) --- */
  memset(bitvec[height],0,64);	/* first zero out all bits */
  for ( icol=0; icol<ncols; icol++ ) /* now check line[] for asterisks */
    if ( line[icol] == '*' )	/* we want to set this bit */
      {	setlongbit(bitvec[height],icol); /* set bit */
	if ( icol >= width ) width=icol+1; } /* and check for new width */
  height++;			/* bump character height */
  } /* --- end-of-while(line!=NULL) --- */
if ( height<1 || width<1 )	/* some problem parsing character */
  goto end_of_job;		/* so quit */
/* --------------------------------------------------------------------------
allocate image raster pixmap for character
-------------------------------------------------------------------------- */
if ( image->pixmap != NULL )	/* hmm, somebody already allocated memory */
  free((void *)image->pixmap);	/* just free it */
image->width = width;		/* set image width within raster struct */
image->height = height;		/* and height */
image->pixsz = pixsz;		/* #bits per pixel, 1=bitmap or 8=bytemap */
if ( (image->pixmap = (unsigned char *)malloc(pixmapsz(image)))
== NULL ) goto end_of_job;	/* quit if failed to allocate pixmap */
/* --------------------------------------------------------------------------
copy each integer in bitvec[] to raster pixmap, bit by bit
-------------------------------------------------------------------------- */
for ( iscan=0; iscan<height; iscan++ )	/* for each integer in vector[] */
  for ( ibit=0; ibit<width; ibit++ )	/* for all bits in this scan */
    {
    if ( getlongbit(bitvec[iscan],ibit) != 0 ) /* check current scan pixel */
      { setlongbit(image->pixmap,ipixel); }
    else				/*turn off corresponding raster bit*/
      { unsetlongbit(image->pixmap,ipixel); }
    ipixel++;				/* bump image raster pixel */
    } /* --- end-of-for(iscan,ibit) --- */
/* --------------------------------------------------------------------------
done
-------------------------------------------------------------------------- */
isokay = 1;				/* reset flag for success */
 end_of_job:
  return ( isokay );			/* back with 1=success, 0=failure */
} /* --- end-of-function rasterizechar() --- */


/* ==========================================================================
 * Function:	parsecorner ( line, row, col )
 * Purpose:	Parses a "pixel corner" line (upper left or lower left)
 *		and returns the (col,row) information on it as integers.
 * --------------------------------------------------------------------------
 * Arguments:	line (I)	char *  to input line containing
 *				".<--This pixel's..." to be parsed
 *		row (O)		int *  returning the (,row)
 *		col (O)		int *  returning the (col,)
 * Returns:	( int )		1 if successful, or 0 for any error
 * --------------------------------------------------------------------------
 * Notes:     o
 * ======================================================================= */
/* --- entry point --- */
int	parsecorner ( char *line, int *row, int *col )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
int	isokay = 0;		/* success/fail flag, init for failure */
char	field[99], *delim;	/*(col,row) field and ptr to various delims*/
/* --------------------------------------------------------------------------
extract (col,row) field from line, and interpret col and row as integers
-------------------------------------------------------------------------- */
/* --- first, check beginning of line --- */
if ( line == (char *)NULL ) goto end_of_job; /* no line supplied by caller */
if ( memcmp(line,CORNER_STUB,strlen(CORNER_STUB)) != 0 ) /*not valid corner*/
  goto end_of_job;			/* so quit */
/* --- extract  col,row  field from line --- */
if ( (delim=strchr(line,'(')) == NULL ) goto end_of_job; /*find open paren*/
strncpy(field,delim+1,10);		/* extract next 10 chars */
field[10] = '\000';			/* and null-terminate field */
if ( (delim=strchr(field,')')) == NULL ) goto end_of_job; /*find close paren*/
*delim = '\000';			/* terminate field at close paren */
/* --- interpret col,row as integers --- */
if ( (delim=strchr(field,',')) == NULL ) goto end_of_job; /* find comma */
*delim = '\000';			/* break field into col and row */
if ( col != (int *)NULL )		/* caller gave us ptr for col */
  *col = atoi(field);			/* so return it to him */
if ( row != (int *)NULL )		/* caller gave us ptr for row */
  *row = atoi(delim+1);			/* so return it to him */
/* --------------------------------------------------------------------------
done
-------------------------------------------------------------------------- */
isokay = 1;				/* reset flag for success */
 end_of_job:
  return ( isokay );			/* back with success/fail flag */
} /* --- end-of-function parsecorner() --- */


/* ==========================================================================
 * Function:	readaline ( fp )
 * Purpose:	Reads a line from fp, strips terminating newline,
 *		and returns ptr to internal buffer
 * --------------------------------------------------------------------------
 * Arguments:	fp (I)		FILE *  to input file to be read.
 *				If null, returns line previously read.
 * Returns:	( char * )	internal buffer containing line read,
 *				or NULL for eof or error.
 * --------------------------------------------------------------------------
 * Notes:     o	fp is left on the line following the returned line
 * ======================================================================= */
/* --- entry point --- */
char	*readaline ( FILE *fp )
{
/* --------------------------------------------------------------------------
Allocations and Declarations
-------------------------------------------------------------------------- */
static	char buffer[1024];	/* static buffer if caller supplies none */
char	*fgets(), *bufptr=buffer; /* read line from fp */
char	*strchr(), *delim;	/* remove terminating newline */
/* --------------------------------------------------------------------------
Read line and strip trailing newline
-------------------------------------------------------------------------- */
if ( fp != NULL )			/*if null, return previous line read*/
  if ( (bufptr=fgets(buffer,1023,fp))	/* read next line from fp */
  != NULL )				/* and check that we succeeded */
    {
    if ( (delim=strchr(bufptr,'\n'))	/* look for terminating newline */
    != NULL )				/* and check that we found it */
      *delim = '\000';			/* truncate line at newline */
    } /* --- end-of-if(fgets()!=NULL) --- */
return ( bufptr );			/*back to caller with buffer or null*/
} /* --- end-of-function readaline() --- */
/* --- end-of-file gfuntype.c --- */

