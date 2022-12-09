var gulp = require('gulp'),
    cache = require('gulp-cached'); //If cached version identical to current file then it doesn't pass it downstream so this file won't be copied 

function copyFiles() {
    return gulp.src('node_modules/**/powerbi.min.js')
            .pipe(cache('node_modules'))
            .pipe(gulp.dest('wwwroot/js'));
}

gulp.task('default', copyFiles);
