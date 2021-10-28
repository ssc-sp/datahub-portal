var gulp = require('gulp'),
    cache = require('gulp-cached'); //If cached version identical to current file then it doesn't pass it downstream so this file won't be copied 

gulp.task('default', 'copy-node_modules');

gulp.task('copy-node_modules', function () {

    try {

        gulp.src('node_modules/**')
            .pipe(cache('node_modules'))
            .pipe(gulp.dest('wwwroot/node_modules'));
    }
    catch (e) {
        return -1;
    }
    return 0;
});
