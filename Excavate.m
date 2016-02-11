str='http://arxiv.org/list/stat.ML/recent';
[sourcefile, status] =urlread(sprintf(str),'get',''); 
expr1 = '<a href="/pdf/\d\d\d\d.\d\d\d\d\d" title="Download PDF">pdf</a>'; 
[datefile, date_tokens]= regexp(sourcefile, expr1, 'match', 'tokens');
l=size(datefile);
for i=1:l(2)
    id=strcat(datefile{1,i}(15),datefile{1,i}(16),datefile{1,i}(17),datefile{1,i}(18),datefile{1,i}(19),datefile{1,i}(20),datefile{1,i}(21),datefile{1,i}(22),datefile{1,i}(23),datefile{1,i}(24));
    %list{1,i}=id;
    https=strcat('http://arxiv.org/pdf/',id,'.pdf');
    path=strcat('P:\Research Project\Thesis\ML\',id,'.pdf')
    sgc_exist = exist(path, 'file');
    if sgc_exist==0
        urlwrite(https,path);
    else
    end
end
