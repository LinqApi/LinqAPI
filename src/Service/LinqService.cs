using AutoMapper;
using LinqApi.Model;
using LinqApi.Repository;
using System.Linq.Expressions;

namespace LinqApi.Service
{
    public class LinqService<T1, T2, TId> : ILinqService<T1, T2, TId>
     where T1 : class
     where T2 : class
    {
        private readonly ILinqRepository<T1, TId> _repository;
        private readonly IMapper _mapper;

        public LinqService(ILinqRepository<T1, TId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<T2> InsertAsync(T2 dto, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<T1>(dto);
            var insertedEntity = await _repository.InsertAsync(entity, cancellationToken);
            return _mapper.Map<T2>(insertedEntity);
        }

        public async Task<T2> UpdateAsync(T2 dto, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<T1>(dto);
            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<T2>(updatedEntity);
        }

        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken);
        }

        public async Task<T2> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return _mapper.Map<T2>(entity);
        }

        public async Task<IEnumerable<T2>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<T2>>(entities);
        }

        public async Task<IEnumerable<T2>> FindAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entities = await _repository.FindAsync(predicate, cancellationToken);
            return _mapper.Map<IEnumerable<T2>>(entities);
        }

        public async Task<PaginationModel<T2>> GetPagedFilteredAsync(
             Expression<Func<T1, bool>> predicate,
             int pageNumber,
             int pageSize,
             List<string> includes = null,
             Expression<Func<T1, object>> orderBy = null,
             bool descending = false,
             CancellationToken cancellationToken = default)
        {
            var pagedResult = await _repository.GetPagedFilteredAsync(predicate, pageNumber, pageSize, includes, orderBy, descending, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<T2>>(pagedResult.Items);
            return new PaginationModel<T2>
            {
                Items = dtos.ToList(),
                TotalRecords = pagedResult.TotalRecords
            };
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.SaveChangesAsync(cancellationToken);
        }
    }

}
