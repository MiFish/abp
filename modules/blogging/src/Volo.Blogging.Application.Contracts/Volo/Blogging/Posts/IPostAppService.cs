﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Volo.Blogging.Posts
{
    public interface IPostAppService : IApplicationService
    {
        Task<ListResultDto<PostWithDetailsDto>> GetListByBlogId(Guid id);

        Task<PostWithDetailsDto> GetByUrlAsync(GetPostInput input);

        Task<PostWithDetailsDto> GetAsync(Guid id);

        Task<PostWithDetailsDto> CreateAsync(CreatePostDto input);

        Task<PostWithDetailsDto> UpdateAsync(Guid id, UpdatePostDto input);
    }
}
